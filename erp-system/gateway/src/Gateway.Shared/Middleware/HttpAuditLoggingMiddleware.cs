using AuditService.Domain.Contracts;
using AuditService.Domain.Enums;
using AuditService.Domain.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;

namespace Gateway.Shared.Middleware;

/// <summary>
/// Middleware for auditing HTTP requests and responses
/// Captures HTTP-level audit information for all requests
/// </summary>
public class HttpAuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuditEventPublisher _auditEventPublisher;
    private readonly ILogger<HttpAuditLoggingMiddleware> _logger;

    public HttpAuditLoggingMiddleware(
        RequestDelegate next,
        IAuditEventPublisher auditEventPublisher,
        ILogger<HttpAuditLoggingMiddleware> logger)
    {
        _next = next;
        _auditEventPublisher = auditEventPublisher;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip audit logging for health checks and internal endpoints
        if (ShouldSkipAuditLogging(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var correlationId = GetOrCreateCorrelationId(context);
        
        // Capture request information
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        Exception? exception = null;
        
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            exception = ex;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Create and publish audit event
            try
            {
                var auditEvent = CreateHttpAuditEvent(context, correlationId, stopwatch.ElapsedMilliseconds, exception);
                await _auditEventPublisher.PublishAsync(auditEvent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish HTTP audit event for request: {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
            }

            // Copy response body back to original stream
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
    }

    private static bool ShouldSkipAuditLogging(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        
        // Skip health checks, metrics, and other operational endpoints
        var skipPaths = new[]
        {
            "/health",
            "/metrics",
            "/swagger",
            "/favicon.ico",
            "/_vs/",
            "/api/audit/health" // Don't audit the audit service health check
        };

        return skipPaths.Any(skipPath => path.StartsWith(skipPath));
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get existing correlation ID from headers
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                          ?? context.Request.Headers["X-Request-ID"].FirstOrDefault()
                          ?? context.TraceIdentifier;

        // Ensure we have a correlation ID
        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Set it in response headers for client visibility
        context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);

        return correlationId;
    }

    private AuditEvent CreateHttpAuditEvent(HttpContext context, string correlationId, long elapsedMs, Exception? exception)
    {
        var request = context.Request;
        var response = context.Response;
        
        var auditEvent = new AuditEvent
        {
            CorrelationId = correlationId,
            ServiceName = GetServiceName(),
            ActionType = DetermineActionType(request.Method),
            Description = $"{request.Method} {request.Path}",
            IpAddress = GetClientIpAddress(context),
            UserAgent = request.Headers["User-Agent"].FirstOrDefault(),
            Source = GetServiceName(),
            DurationMs = elapsedMs,
            Metadata = CreateMetadata(context)
        };

        // Set result and severity based on response or exception
        if (exception != null)
        {
            auditEvent.Result = AuditResult.Failed;
            auditEvent.Severity = AuditSeverity.Error;
            auditEvent.ErrorMessage = exception.Message;
            auditEvent.Description = $"Failed {request.Method} {request.Path}: {exception.Message}";
        }
        else
        {
            auditEvent.Result = response.StatusCode < 400 ? AuditResult.Success : AuditResult.Failed;
            auditEvent.Severity = response.StatusCode switch
            {
                >= 500 => AuditSeverity.Error,
                >= 400 => AuditSeverity.Warning,
                _ => AuditSeverity.Information
            };

            if (response.StatusCode >= 400)
            {
                auditEvent.Description = $"{request.Method} {request.Path} returned {response.StatusCode}";
            }
        }

        // Extract user information
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            auditEvent.UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            auditEvent.UserDisplayName = context.User.FindFirst(ClaimTypes.Name)?.Value 
                ?? context.User.FindFirst("name")?.Value;
            auditEvent.TenantId = context.User.FindFirst("tid")?.Value;
        }

        // Determine retention category
        auditEvent.RetentionCategory = DetermineRetentionCategory(request.Path, auditEvent.ActionType);

        // Check for sensitive data
        auditEvent.ContainsSensitiveData = ContainsSensitiveData(request.Path);

        return auditEvent;
    }

    private static string GetServiceName()
    {
        return Environment.GetEnvironmentVariable("SERVICE_NAME") ?? "Gateway";
    }

    private static AuditActionType DetermineActionType(string httpMethod)
    {
        return httpMethod.ToUpperInvariant() switch
        {
            "GET" => AuditActionType.Read,
            "POST" => AuditActionType.Create,
            "PUT" or "PATCH" => AuditActionType.Update,
            "DELETE" => AuditActionType.Delete,
            _ => AuditActionType.Other
        };
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        // Try to get IP from X-Forwarded-For header first (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Try X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return context.Connection.RemoteIpAddress?.ToString();
    }

    private static string CreateMetadata(HttpContext context)
    {
        var metadata = new
        {
            RequestPath = context.Request.Path.Value,
            QueryString = context.Request.QueryString.Value,
            Method = context.Request.Method,
            StatusCode = context.Response.StatusCode,
            RequestSize = context.Request.ContentLength,
            ResponseSize = context.Response.ContentLength,
            Protocol = context.Request.Protocol,
            Scheme = context.Request.Scheme,
            Host = context.Request.Host.Value,
            Referer = context.Request.Headers["Referer"].FirstOrDefault(),
            Headers = GetSafeHeaders(context.Request.Headers)
        };

        return System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    private static Dictionary<string, string> GetSafeHeaders(IHeaderDictionary headers)
    {
        // Only include safe headers, exclude sensitive ones
        var safeHeaders = new[] { "Accept", "Accept-Language", "Content-Type", "X-Requested-With" };
        var result = new Dictionary<string, string>();

        foreach (var header in headers)
        {
            if (safeHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
            {
                result[header.Key] = header.Value.ToString();
            }
        }

        return result;
    }

    private static string DetermineRetentionCategory(PathString path, AuditActionType actionType)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;

        if (pathValue.Contains("/hr/") || pathValue.Contains("/employee"))
        {
            return "hr";
        }

        if (pathValue.Contains("/accounting/") || pathValue.Contains("/finance"))
        {
            return "financial";
        }

        if (pathValue.Contains("/auth") || pathValue.Contains("/login"))
        {
            return "security";
        }

        return actionType switch
        {
            AuditActionType.Delete => "audit",
            _ => "operational"
        };
    }

    private static bool ContainsSensitiveData(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? string.Empty;
        
        var sensitivePatterns = new[] { "password", "secret", "token", "salary", "ssn", "personal" };
        return sensitivePatterns.Any(pattern => pathValue.Contains(pattern));
    }
}

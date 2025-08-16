using AuditService.Domain.Contracts;
using AuditService.Domain.Enums;
using AuditService.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;

namespace Gateway.Shared.Behaviors;

/// <summary>
/// MediatR pipeline behavior for automatic audit logging
/// Captures request and response data for all MediatR commands and queries
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class AuditLoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditEventPublisher _auditEventPublisher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditLoggingBehavior<TRequest, TResponse>> _logger;

    public AuditLoggingBehavior(
        IAuditEventPublisher auditEventPublisher,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditLoggingBehavior<TRequest, TResponse>> logger)
    {
        _auditEventPublisher = auditEventPublisher;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var httpContext = _httpContextAccessor.HttpContext;
        var correlationId = GetCorrelationId(httpContext);
        
        AuditEvent? auditEvent = null;
        TResponse? response = default;
        Exception? exception = null;

        try
        {
            // Create audit event for the request
            auditEvent = CreateAuditEvent(request, httpContext, correlationId);

            // Execute the request
            response = await next();

            // Update audit event with response data
            if (auditEvent != null)
            {
                UpdateAuditEventWithResponse(auditEvent, response, stopwatch.ElapsedMilliseconds);
            }

            return response;
        }
        catch (Exception ex)
        {
            exception = ex;
            
            // Update audit event with error information
            if (auditEvent != null)
            {
                UpdateAuditEventWithError(auditEvent, ex, stopwatch.ElapsedMilliseconds);
            }

            throw;
        }
        finally
        {
            stopwatch.Stop();

            // Publish audit event
            if (auditEvent != null)
            {
                try
                {
                    await _auditEventPublisher.PublishAsync(auditEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to publish audit event for request: {RequestType}", typeof(TRequest).Name);
                    // Don't rethrow - audit logging should not break the main flow
                }
            }
        }
    }

    private static string GetCorrelationId(HttpContext? httpContext)
    {
        if (httpContext?.TraceIdentifier != null)
        {
            return httpContext.TraceIdentifier;
        }

        // Fallback to generating a new correlation ID
        return Guid.NewGuid().ToString();
    }

    private AuditEvent? CreateAuditEvent(TRequest request, HttpContext? httpContext, string correlationId)
    {
        try
        {
            var requestType = typeof(TRequest);
            var serviceName = GetServiceName(httpContext);
            var actionType = DetermineActionType(requestType.Name);

            var auditEvent = new AuditEvent
            {
                CorrelationId = correlationId,
                ServiceName = serviceName,
                ActionType = actionType,
                EntityType = ExtractEntityType(requestType),
                Description = $"Executed {requestType.Name}",
                IpAddress = GetClientIpAddress(httpContext),
                UserAgent = httpContext?.Request.Headers["User-Agent"].FirstOrDefault(),
                Source = serviceName,
                Severity = AuditSeverity.Information,
                Result = AuditResult.Success // Will be updated based on outcome
            };

            // Extract user information from claims
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                auditEvent.UserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                auditEvent.UserDisplayName = httpContext.User.FindFirst(ClaimTypes.Name)?.Value 
                    ?? httpContext.User.FindFirst("name")?.Value;
                auditEvent.TenantId = httpContext.User.FindFirst("tid")?.Value;
            }

            // Serialize request data (be careful with sensitive information)
            if (ShouldSerializeRequest(requestType))
            {
                auditEvent.NewData = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
            }

            // Set retention category based on action type
            auditEvent.RetentionCategory = DetermineRetentionCategory(actionType, requestType);

            // Mark sensitive data if needed
            auditEvent.ContainsSensitiveData = ContainsSensitiveData(requestType);

            return auditEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating audit event for request: {RequestType}", typeof(TRequest).Name);
            return null;
        }
    }

    private static void UpdateAuditEventWithResponse(AuditEvent auditEvent, TResponse response, long elapsedMs)
    {
        auditEvent.Result = AuditResult.Success;
        auditEvent.DurationMs = elapsedMs;

        // Optionally serialize response (be very careful with sensitive data)
        if (ShouldSerializeResponse(typeof(TResponse)) && response != null)
        {
            try
            {
                auditEvent.Metadata = JsonSerializer.Serialize(new { ResponseType = typeof(TResponse).Name }, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch (Exception)
            {
                // Ignore serialization errors for response data
            }
        }
    }

    private static void UpdateAuditEventWithError(AuditEvent auditEvent, Exception exception, long elapsedMs)
    {
        auditEvent.Result = AuditResult.Failed;
        auditEvent.Severity = AuditSeverity.Error;
        auditEvent.DurationMs = elapsedMs;
        auditEvent.ErrorMessage = exception.Message;
        auditEvent.Description = $"Failed to execute {typeof(TRequest).Name}: {exception.Message}";
    }

    private static string GetServiceName(HttpContext? httpContext)
    {
        // Try to get service name from configuration or environment
        return Environment.GetEnvironmentVariable("SERVICE_NAME") 
            ?? httpContext?.Request.Host.Host 
            ?? "UnknownService";
    }

    private static AuditActionType DetermineActionType(string requestName)
    {
        return requestName.ToLowerInvariant() switch
        {
            var name when name.Contains("create") || name.Contains("add") => AuditActionType.Create,
            var name when name.Contains("update") || name.Contains("modify") || name.Contains("edit") => AuditActionType.Update,
            var name when name.Contains("delete") || name.Contains("remove") => AuditActionType.Delete,
            var name when name.Contains("get") || name.Contains("query") || name.Contains("search") || name.Contains("list") => AuditActionType.Read,
            var name when name.Contains("login") || name.Contains("signin") => AuditActionType.Login,
            var name when name.Contains("logout") || name.Contains("signout") => AuditActionType.Logout,
            var name when name.Contains("export") => AuditActionType.Export,
            var name when name.Contains("import") => AuditActionType.Import,
            _ => AuditActionType.Other
        };
    }

    private static string? ExtractEntityType(Type requestType)
    {
        var typeName = requestType.Name;
        
        // Common patterns for extracting entity type from request names
        if (typeName.EndsWith("Command") || typeName.EndsWith("Query"))
        {
            var prefixes = new[] { "Create", "Update", "Delete", "Get", "List", "Search" };
            var suffixes = new[] { "Command", "Query" };

            var entityName = typeName;
            
            foreach (var prefix in prefixes)
            {
                if (entityName.StartsWith(prefix))
                {
                    entityName = entityName.Substring(prefix.Length);
                    break;
                }
            }

            foreach (var suffix in suffixes)
            {
                if (entityName.EndsWith(suffix))
                {
                    entityName = entityName.Substring(0, entityName.Length - suffix.Length);
                    break;
                }
            }

            return string.IsNullOrEmpty(entityName) ? null : entityName;
        }

        return null;
    }

    private static string? GetClientIpAddress(HttpContext? httpContext)
    {
        if (httpContext == null) return null;

        // Try to get IP from X-Forwarded-For header first (for load balancers/proxies)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        // Try X-Real-IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to connection remote IP
        return httpContext.Connection.RemoteIpAddress?.ToString();
    }

    private static bool ShouldSerializeRequest(Type requestType)
    {
        // Don't serialize query requests or requests containing sensitive data
        var typeName = requestType.Name.ToLowerInvariant();
        
        if (typeName.Contains("query") || typeName.Contains("get") || typeName.Contains("list"))
        {
            return false;
        }

        if (typeName.Contains("password") || typeName.Contains("secret") || typeName.Contains("token"))
        {
            return false;
        }

        return true;
    }

    private static bool ShouldSerializeResponse(Type responseType)
    {
        // Generally don't serialize responses to avoid storing large amounts of data
        // Only serialize for specific scenarios where needed for compliance
        return false;
    }

    private static string DetermineRetentionCategory(AuditActionType actionType, Type requestType)
    {
        var typeName = requestType.Name.ToLowerInvariant();

        if (typeName.Contains("employee") || typeName.Contains("hr"))
        {
            return "hr";
        }

        if (typeName.Contains("financial") || typeName.Contains("accounting") || typeName.Contains("payment"))
        {
            return "financial";
        }

        if (typeName.Contains("login") || typeName.Contains("auth") || typeName.Contains("security"))
        {
            return "security";
        }

        return actionType switch
        {
            AuditActionType.Login or AuditActionType.Logout or AuditActionType.AccessChange => "security",
            AuditActionType.Delete => "audit",
            _ => "operational"
        };
    }

    private static bool ContainsSensitiveData(Type requestType)
    {
        var typeName = requestType.Name.ToLowerInvariant();
        
        return typeName.Contains("password") || 
               typeName.Contains("secret") || 
               typeName.Contains("token") ||
               typeName.Contains("salary") ||
               typeName.Contains("ssn") ||
               typeName.Contains("personal");
    }
}

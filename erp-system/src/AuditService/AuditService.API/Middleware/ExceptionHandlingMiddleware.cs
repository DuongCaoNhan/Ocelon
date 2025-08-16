using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace AuditService.API.Middleware;

/// <summary>
/// Global exception handling middleware for the audit service
/// Provides consistent error responses and logging
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request processing");
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = new
            {
                message = "An error occurred while processing your request.",
                type = exception.GetType().Name,
                traceId = context.TraceIdentifier
            }
        };

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentOutOfRangeException:
            case ArgumentException:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new
                {
                    error = new
                    {
                        message = exception.Message,
                        type = exception.GetType().Name,
                        traceId = context.TraceIdentifier
                    }
                };
                break;

            case UnauthorizedAccessException:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response = new
                {
                    error = new
                    {
                        message = "Access denied to audit logs.",
                        type = exception.GetType().Name,
                        traceId = context.TraceIdentifier
                    }
                };
                break;

            case TimeoutException:
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                response = new
                {
                    error = new
                    {
                        message = "The request timed out.",
                        type = exception.GetType().Name,
                        traceId = context.TraceIdentifier
                    }
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

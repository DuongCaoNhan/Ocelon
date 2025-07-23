using System.Net;
using System.Text.Json;

namespace AIAgentService.API.Middleware
{
    /// <summary>
    /// Global exception handling middleware for consistent error responses
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case ArgumentNullException:
                case ArgumentException:
                    response.Message = "Invalid request parameters";
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case InvalidOperationException:
                    response.Message = exception.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case UnauthorizedAccessException:
                    response.Message = "Unauthorized access";
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                default:
                    response.Message = "An internal server error occurred";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            response.Instance = context.Request.Path;
            response.TraceId = context.TraceIdentifier;

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string? Instance { get; set; }
        public string? TraceId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

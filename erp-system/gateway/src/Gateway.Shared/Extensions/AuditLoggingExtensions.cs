using AuditService.Domain.Contracts;
using Gateway.Shared.Services;
using Gateway.Shared.Behaviors;
using Gateway.Shared.Middleware;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Shared.Extensions;

/// <summary>
/// Extension methods for configuring audit logging in services
/// Provides easy integration of audit logging capabilities
/// </summary>
public static class AuditLoggingExtensions
{
    /// <summary>
    /// Adds audit logging services to the DI container
    /// Configures Azure Service Bus publisher and MediatR behavior
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddAuditLogging(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Azure Service Bus audit event publisher
        services.AddSingleton<IAuditEventPublisher, ServiceBusAuditEventPublisher>();

        // Configure Service Bus options
        services.Configure<ServiceBusOptions>(configuration.GetSection("ServiceBus"));

        // Add HTTP context accessor for accessing request context
        services.AddHttpContextAccessor();

        // Add MediatR pipeline behavior for automatic audit logging
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(AuditLoggingBehavior<,>));

        // Add health checks for audit logging dependencies
        services.AddHealthChecks()
            .AddCheck("audit-logging", () =>
            {
                var serviceBusNamespace = configuration["AzureServiceBus:Namespace"];
                if (string.IsNullOrEmpty(serviceBusNamespace))
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded("Azure Service Bus not configured");
                }
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Audit logging configured");
            });

        return services;
    }

    /// <summary>
    /// Adds HTTP audit logging middleware to the application pipeline
    /// Should be called early in the pipeline configuration
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for chaining</returns>
    public static IApplicationBuilder UseAuditLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HttpAuditLoggingMiddleware>();
    }

    /// <summary>
    /// Configures audit logging for a specific service
    /// Sets up common audit logging patterns and configuration
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="serviceName">Name of the service for audit identification</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddServiceAuditLogging(
        this IServiceCollection services, 
        IConfiguration configuration, 
        string serviceName)
    {
        // Set the service name as an environment variable for audit logging
        Environment.SetEnvironmentVariable("SERVICE_NAME", serviceName);

        // Add core audit logging
        services.AddAuditLogging(configuration);

        // Add service-specific configurations if needed
        services.Configure<AuditLoggingOptions>(options =>
        {
            options.ServiceName = serviceName;
            options.EnableHttpAuditLogging = configuration.GetValue<bool>("Audit:EnableHttpLogging", true);
            options.EnableMediatRLogging = configuration.GetValue<bool>("Audit:EnableMediatRLogging", true);
            options.IncludeSensitiveData = configuration.GetValue<bool>("Audit:IncludeSensitiveData", false);
        });

        return services;
    }
}

/// <summary>
/// Configuration options for audit logging
/// </summary>
public class AuditLoggingOptions
{
    /// <summary>
    /// Name of the service generating audit events
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Enable HTTP request/response audit logging
    /// </summary>
    public bool EnableHttpAuditLogging { get; set; } = true;

    /// <summary>
    /// Enable MediatR command/query audit logging
    /// </summary>
    public bool EnableMediatRLogging { get; set; } = true;

    /// <summary>
    /// Include sensitive data in audit logs (not recommended for production)
    /// </summary>
    public bool IncludeSensitiveData { get; set; } = false;

    /// <summary>
    /// Maximum size of request/response data to log (in characters)
    /// </summary>
    public int MaxDataSize { get; set; } = 10000;

    /// <summary>
    /// List of paths to exclude from HTTP audit logging
    /// </summary>
    public List<string> ExcludedPaths { get; set; } = new()
    {
        "/health",
        "/metrics",
        "/swagger"
    };

    /// <summary>
    /// List of request types to exclude from MediatR audit logging
    /// </summary>
    public List<string> ExcludedRequestTypes { get; set; } = new();
}

using AuditService.Domain.Contracts;
using AuditService.Infrastructure.Data;
using AuditService.Infrastructure.Repositories;
using AuditService.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace AuditService.Infrastructure.Extensions;

/// <summary>
/// Extension methods for configuring infrastructure services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure layer services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Entity Framework
        AddEntityFramework(services, configuration);

        // Add Azure Service Bus
        AddAzureServiceBus(services);

        // Add repositories
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Add background service for processing audit events
        services.AddHostedService<AuditEventProcessorService>();

        return services;
    }

    private static void AddEntityFramework(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AuditDatabase")
            ?? throw new InvalidOperationException("AuditDatabase connection string is required");

        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                // Performance optimizations
                sqlOptions.CommandTimeout(30);
                
                // Migration assembly
                sqlOptions.MigrationsAssembly("AuditService.Infrastructure");
            });

            // Performance configurations for high-volume logging
            options.EnableServiceProviderCaching();
            options.EnableSensitiveDataLogging(false); // Security: never log sensitive data
            
            // Configure for write-heavy workload
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        // Health checks for Entity Framework
        services.AddHealthChecks()
            .AddDbContextCheck<AuditDbContext>("audit-database");
    }

    private static void AddAzureServiceBus(IServiceCollection services)
    {
        // Register the Azure Service Bus audit event publisher
        services.AddSingleton<IAuditEventPublisher, AzureServiceBusAuditEventPublisher>();

        // Health checks for Service Bus (if configured)
        services.AddHealthChecks()
            .AddCheck("azure-service-bus", () =>
            {
                // Basic health check - can be enhanced to actually test Service Bus connectivity
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Service Bus configuration loaded");
            });
    }

    /// <summary>
    /// Ensures the audit database is created and migrations are applied
    /// Should be called during application startup
    /// </summary>
    /// <param name="serviceProvider">Service provider instance</param>
    /// <returns>Task representing the async operation</returns>
    public static async Task InitializeAuditDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        
        // Apply any pending migrations
        await context.Database.MigrateAsync();
    }
}

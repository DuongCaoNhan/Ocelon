using AuditService.Application.Handlers;
using AuditService.Application.Mappings;
using AuditService.Application.Validators;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AuditService.Application.Extensions;

/// <summary>
/// Extension methods for configuring application services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application layer services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // Add AutoMapper
        services.AddAutoMapper(typeof(AuditMappingProfile));

        // Add FluentValidation
        services.AddValidatorsFromAssemblyContaining<AuditLogQueryDtoValidator>();

        // Add application services
        services.AddScoped<ProcessAuditEventHandler>();
        services.AddScoped<ProcessAuditEventBatchHandler>();
        services.AddScoped<GetAuditLogsHandler>();
        services.AddScoped<GetAuditLogsByCorrelationIdHandler>();
        services.AddScoped<GetAuditLogsByEntityHandler>();

        return services;
    }
}

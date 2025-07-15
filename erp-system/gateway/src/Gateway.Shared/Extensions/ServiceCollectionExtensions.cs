using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Exporter;
using Gateway.Shared.Services;
using System.Text;

namespace Gateway.Shared.Extensions;

/// <summary>
/// Extension methods for configuring common gateway services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT authentication to the service collection
    /// </summary>
    public static IServiceCollection AddGatewayAuthentication(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Authentication:Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is required");
        var issuer = jwtSettings["Issuer"] ?? "ocelon-erp";
        var audience = jwtSettings["Audience"] ?? "ocelon-erp-api";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Log.Debug("JWT Token validated for user: {User}", 
                            context.Principal?.Identity?.Name ?? "Unknown");
                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }

    /// <summary>
    /// Adds basic rate limiting configuration (simplified for compatibility)
    /// </summary>
    public static IServiceCollection AddGatewayRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Note: For .NET 8 compatibility, implement custom rate limiting
        // or use third-party libraries like AspNetCoreRateLimit
        
        // Basic implementation - can be extended with proper rate limiting middleware
        services.AddSingleton<IRateLimitService, BasicRateLimitService>();
        
        return services;
    }

    /// <summary>
    /// Adds distributed tracing with OpenTelemetry
    /// </summary>
    public static IServiceCollection AddGatewayTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName = "ocelon-gateway")
    {
        var telemetrySettings = configuration.GetSection("Telemetry");
        var endpoint = telemetrySettings["Endpoint"];
        var enabled = telemetrySettings.GetValue<bool>("Enabled", true);

        if (!enabled)
            return services;

        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: "1.0.0")
                        .AddAttributes(new Dictionary<string, object>
                        {
                            ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                            ["service.instance.id"] = Environment.MachineName
                        }))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.Filter = httpContext => !httpContext.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation();

                if (!string.IsNullOrEmpty(endpoint))
                {
                    // Note: AddOtlpExporter may require specific package versions
                    // For now, just use console exporter for compatibility
                    builder.AddConsoleExporter();
                }
                else
                {
                    builder.AddConsoleExporter();
                }
            });

        return services;
    }

    /// <summary>
    /// Adds structured logging with Serilog
    /// </summary>
    public static IHostBuilder AddGatewayLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("ServiceName", "ocelon-gateway")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithProperty("MachineName", Environment.MachineName);
        });
    }

    /// <summary>
    /// Adds CORS policy for the gateway
    /// </summary>
    public static IServiceCollection AddGatewayCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsSettings = configuration.GetSection("Cors");
        var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" };
        var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" };

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                if (allowedOrigins.Contains("*"))
                {
                    builder.AllowAnyOrigin();
                }
                else
                {
                    builder.WithOrigins(allowedOrigins);
                }

                if (allowedMethods.Contains("*"))
                {
                    builder.AllowAnyMethod();
                }
                else
                {
                    builder.WithMethods(allowedMethods);
                }

                if (allowedHeaders.Contains("*"))
                {
                    builder.AllowAnyHeader();
                }
                else
                {
                    builder.WithHeaders(allowedHeaders);
                }

                builder.AllowCredentials();
            });
        });

        return services;
    }
}

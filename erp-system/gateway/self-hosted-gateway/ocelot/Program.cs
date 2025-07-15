using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Add Swagger for the gateway itself
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "ERP Ocelot Gateway", 
        Version = "v1",
        Description = "Ocelot API Gateway for Ocelon ERP Management System"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("ERPCorsPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001", 
                "http://localhost:4200",
                "https://erp-portal.com",
                "https://admin.erp-portal.com"
              )
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://login.microsoftonline.com/your-tenant-id";
        options.Audience = "your-client-id";
        options.RequireHttpsMetadata = false; // Set to true in production
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError("Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Token validated for user: {User}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
    });

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("ocelot-gateway", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Ocelot Gateway is running"));

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Add Ocelot
builder.Services.AddOcelot()
    .AddConsul()
    .AddPolly();

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddApplicationInsights();
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Ocelot Gateway v1");
        c.RoutePrefix = "gateway-docs";
        
        // Add service-specific Swagger endpoints
        c.SwaggerEndpoint("/swagger/hr-service/swagger.json", "HR Service API");
        c.SwaggerEndpoint("/swagger/inventory-service/swagger.json", "Inventory Service API");
        c.SwaggerEndpoint("/swagger/accounting-service/swagger.json", "Accounting Service API");
        c.SwaggerEndpoint("/swagger/workflow-service/swagger.json", "Workflow Service API");
        c.SwaggerEndpoint("/swagger/notification-service/swagger.json", "Notification Service API");
    });
}

// Custom middleware for request/response logging
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    
    // Add correlation ID to response
    context.Response.Headers.Add("X-Correlation-ID", correlationId);
    context.Response.Headers.Add("X-Gateway", "Ocelot-Gateway");
    context.Response.Headers.Add("X-Gateway-Version", "1.0.0");
    
    logger.LogInformation("Ocelot Request: {Method} {Path} - Correlation ID: {CorrelationId}", 
        context.Request.Method, context.Request.Path, correlationId);
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        logger.LogInformation("Ocelot Response: {StatusCode} - Duration: {Duration}ms - Correlation ID: {CorrelationId}", 
            context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
    }
});

// Custom middleware for API key authentication
app.Use(async (context, next) =>
{
    var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
    
    if (!string.IsNullOrEmpty(apiKey))
    {
        // Validate API key (implement your validation logic)
        var validApiKeys = new[] { "dev-service-key-12345", "staging-service-key-67890" };
        
        if (validApiKeys.Contains(apiKey))
        {
            var claims = new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "Service Account"),
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, apiKey),
                new System.Security.Claims.Claim("roles", "Service.Account")
            };
            
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "ApiKey");
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }
    }
    
    await next();
});

app.UseHttpsRedirection();
app.UseCors("ERPCorsPolicy");
app.UseAuthentication();

// Health check endpoints
app.MapHealthChecks("/health");

// Gateway info endpoint
app.MapGet("/gateway/info", () => new
{
    name = "ERP Ocelot Gateway",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    features = new[]
    {
        "JWT Authentication",
        "API Key Authentication", 
        "Rate Limiting",
        "Load Balancing",
        "Circuit Breaker",
        "Request/Response Logging",
        "Health Checks",
        "Service Discovery (Consul)",
        "Swagger Integration"
    }
}).AllowAnonymous();

// Routes information endpoint
app.MapGet("/gateway/routes", () => 
{
    var routes = new[]
    {
        new { path = "/v1/hr/**", service = "HR Service", port = 5001 },
        new { path = "/v1/inventory/**", service = "Inventory Service", port = 5002 },
        new { path = "/v1/accounting/**", service = "Accounting Service", port = 5003 },
        new { path = "/v1/workflow/**", service = "Workflow Service", port = 8080 },
        new { path = "/v1/notifications/**", service = "Notification Service", port = 3000 }
    };
    
    return Results.Ok(new { routes, timestamp = DateTime.UtcNow });
}).AllowAnonymous();

// Service health aggregation endpoint
app.MapGet("/health/services", async (HttpClient httpClient) =>
{
    var services = new[]
    {
        new { name = "HR Service", url = "http://localhost:5001/health" },
        new { name = "Inventory Service", url = "http://localhost:5002/health" },
        new { name = "Accounting Service", url = "http://localhost:5003/health" },
        new { name = "Workflow Service", url = "http://localhost:8080/actuator/health" },
        new { name = "Notification Service", url = "http://localhost:3000/health" }
    };

    var healthChecks = new List<object>();

    foreach (var service in services)
    {
        try
        {
            var response = await httpClient.GetAsync(service.url);
            healthChecks.Add(new
            {
                service = service.name,
                status = response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                statusCode = (int)response.StatusCode,
                responseTime = "< 1000ms" // Simplified for demo
            });
        }
        catch
        {
            healthChecks.Add(new
            {
                service = service.name,
                status = "Unhealthy",
                statusCode = 0,
                responseTime = "Timeout"
            });
        }
    }

    var overallStatus = healthChecks.All(h => h.GetType().GetProperty("status")?.GetValue(h)?.ToString() == "Healthy") 
        ? "Healthy" : "Degraded";

    return Results.Ok(new
    {
        status = overallStatus,
        services = healthChecks,
        timestamp = DateTime.UtcNow
    });
}).AllowAnonymous();

// Add Ocelot middleware
await app.UseOcelot();

app.Run();

// Custom health check aggregator
public class HealthCheckAggregator
{
    public async Task<object> Aggregate(List<object> responses)
    {
        var healthyCount = 0;
        var totalCount = responses.Count;

        foreach (var response in responses)
        {
            // Check if response indicates healthy status
            // This is a simplified implementation
            if (response.ToString()?.Contains("Healthy") == true)
            {
                healthyCount++;
            }
        }

        var overallStatus = healthyCount == totalCount ? "Healthy" : 
                           healthyCount > 0 ? "Degraded" : "Unhealthy";

        return new
        {
            status = overallStatus,
            healthyServices = healthyCount,
            totalServices = totalCount,
            services = responses,
            timestamp = DateTime.UtcNow
        };
    }
}

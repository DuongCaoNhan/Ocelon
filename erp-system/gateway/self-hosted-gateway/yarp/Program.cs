using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "ERP Gateway API", 
        Version = "v1",
        Description = "API Gateway for Ocelon ERP Management System"
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("erp-cors", policy =>
    {
        var corsConfig = builder.Configuration.GetSection("CORS");
        var allowedOrigins = corsConfig.GetSection("AllowedOrigins").Get<string[]>() ?? new[] { "*" };
        var allowedMethods = corsConfig.GetSection("AllowedMethods").Get<string[]>() ?? new[] { "*" };
        var allowedHeaders = corsConfig.GetSection("AllowedHeaders").Get<string[]>() ?? new[] { "*" };
        var allowCredentials = corsConfig.GetValue<bool>("AllowCredentials");

        if (allowedOrigins.Contains("*"))
        {
            policy.AllowAnyOrigin();
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }

        if (allowedMethods.Contains("*"))
        {
            policy.AllowAnyMethod();
        }
        else
        {
            policy.WithMethods(allowedMethods);
        }

        if (allowedHeaders.Contains("*"))
        {
            policy.AllowAnyHeader();
        }
        else
        {
            policy.WithHeaders(allowedHeaders);
        }

        if (allowCredentials)
        {
            policy.AllowCredentials();
        }
    });
});

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtConfig = builder.Configuration.GetSection("Authentication:JWT");
    
    options.Authority = jwtConfig["Authority"];
    options.Audience = jwtConfig["Audience"];
    options.RequireHttpsMetadata = jwtConfig.GetValue<bool>("RequireHttpsMetadata", true);
    
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
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for user: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated-users", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("hr-managers", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("roles", "HR.Manager", "Admin");
    });

    options.AddPolicy("admins", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("roles", "Admin");
    });
});

// Add Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Default rate limiting policy
    options.AddFixedWindowLimiter("default-rate-limit", config =>
    {
        var defaultPolicy = builder.Configuration.GetSection("RateLimiting:DefaultPolicy");
        config.PermitLimit = defaultPolicy.GetValue<int>("PermitLimit", 100);
        config.Window = defaultPolicy.GetValue<TimeSpan>("Window", TimeSpan.FromMinutes(1));
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = defaultPolicy.GetValue<int>("QueueLimit", 10);
    });

    // HR service rate limiting
    options.AddFixedWindowLimiter("hr-rate-limit", config =>
    {
        var hrPolicy = builder.Configuration.GetSection("RateLimiting:HRPolicy");
        config.PermitLimit = hrPolicy.GetValue<int>("PermitLimit", 50);
        config.Window = hrPolicy.GetValue<TimeSpan>("Window", TimeSpan.FromMinutes(1));
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = hrPolicy.GetValue<int>("QueueLimit", 5);
    });

    // Notification service rate limiting
    options.AddFixedWindowLimiter("notification-rate-limit", config =>
    {
        var notificationPolicy = builder.Configuration.GetSection("RateLimiting:NotificationPolicy");
        config.PermitLimit = notificationPolicy.GetValue<int>("PermitLimit", 200);
        config.Window = notificationPolicy.GetValue<TimeSpan>("Window", TimeSpan.FromMinutes(1));
        config.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        config.QueueLimit = notificationPolicy.GetValue<int>("QueueLimit", 20);
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Rate limit exceeded",
            details = "Too many requests. Please try again later.",
            retryAfter = "60 seconds",
            traceId = context.HttpContext.TraceIdentifier
        };

        await context.HttpContext.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    };
});

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddCheck("gateway-health", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Gateway is running"));

// Add Application Insights if configured
var appInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(appInsightsConnectionString);
}

// Add logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
    
    if (!string.IsNullOrEmpty(appInsightsConnectionString))
    {
        logging.AddApplicationInsights();
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ERP Gateway API v1");
        c.RoutePrefix = "docs";
    });
}

// Add custom middleware for logging and monitoring
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
    
    context.Response.Headers.Add("X-Correlation-ID", correlationId);
    context.Response.Headers.Add("X-Gateway", "YARP-Gateway");
    context.Response.Headers.Add("X-Gateway-Version", "1.0.0");
    
    logger.LogInformation("Request: {Method} {Path} - Correlation ID: {CorrelationId}", 
        context.Request.Method, context.Request.Path, correlationId);
    
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        logger.LogInformation("Response: {StatusCode} - Duration: {Duration}ms - Correlation ID: {CorrelationId}", 
            context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
    }
});

// API Key authentication middleware
app.Use(async (context, next) =>
{
    var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
    
    if (!string.IsNullOrEmpty(apiKey))
    {
        var validKeys = app.Configuration.GetSection("Authentication:ApiKey:ValidKeys").Get<string[]>() ?? Array.Empty<string>();
        
        if (validKeys.Contains(apiKey))
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "Service Account"),
                new Claim(ClaimTypes.NameIdentifier, apiKey),
                new Claim("roles", "Service.Account")
            };
            
            var identity = new ClaimsIdentity(claims, "ApiKey");
            context.User = new ClaimsPrincipal(identity);
        }
    }
    
    await next();
});

app.UseHttpsRedirection();
app.UseCors("erp-cors");
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Health check endpoint
app.MapHealthChecks("/health");

// Custom gateway endpoints
app.MapGet("/gateway/info", () => new
{
    name = "ERP Gateway",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow
}).AllowAnonymous();

app.MapGet("/gateway/routes", (IConfiguration config) =>
{
    var routes = config.GetSection("ReverseProxy:Routes").GetChildren()
        .Select(route => new
        {
            name = route.Key,
            path = route.GetValue<string>("Match:Path"),
            cluster = route.GetValue<string>("ClusterId"),
            authPolicy = route.GetValue<string>("AuthorizationPolicy"),
            rateLimitPolicy = route.GetValue<string>("RateLimiterPolicy")
        });
    
    return Results.Ok(routes);
}).RequireAuthorization("admins");

// Map the reverse proxy
app.MapReverseProxy();

app.MapControllers();

app.Run();

// Custom API Key authentication handler
public class ApiKeyAuthenticationSchemeOptions : Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions
{
    public string HeaderName { get; set; } = "X-API-Key";
    public string[] ValidKeys { get; set; } = Array.Empty<string>();
}

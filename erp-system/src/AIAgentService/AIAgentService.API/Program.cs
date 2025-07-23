using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Azure.Identity;
using AIAgentService.API.Middleware;
using AIAgentService.Infrastructure.Data;
using AIAgentService.Infrastructure.Repositories;
using AIAgentService.Infrastructure.Services;
using AIAgentService.Application.Services;
using AIAgentService.Application.Handlers;
using AIAgentService.Application.Mappings;
using AIAgentService.Application.Validators;
using AIAgentService.Domain.Repositories;
using AIAgentService.Domain.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using MediatR;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/copilot-agent-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Copilot Agent Service API", Version = "v1" });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "AIAgentService.API.xml"));
});

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<AgentDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Configure Redis Cache for distributed caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "CopilotAgent";
});

// Register MediatR
builder.Services.AddMediatR(cfg => 
{
    cfg.RegisterServicesFromAssembly(typeof(CreateAgentSessionHandler).Assembly);
});

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(AgentMappingProfile));

// Register FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAgentSessionCommandValidator>();

// Configure Semantic Kernel with Azure OpenAI
builder.Services.AddTransient<Kernel>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var logger = serviceProvider.GetRequiredService<ILogger<Kernel>>();

    var kernelBuilder = Kernel.CreateBuilder();

    // Configure Azure OpenAI
    var azureOpenAIEndpoint = configuration["AzureOpenAI:Endpoint"];
    var azureOpenAIDeployment = configuration["AzureOpenAI:DeploymentName"] ?? "gpt-4";

    if (!string.IsNullOrEmpty(azureOpenAIEndpoint))
    {
        // Use Managed Identity for Azure OpenAI authentication
        kernelBuilder.AddAzureOpenAIChatCompletion(
            deploymentName: azureOpenAIDeployment,
            endpoint: azureOpenAIEndpoint,
            credentials: new DefaultAzureCredential());

        logger.LogInformation("Configured Azure OpenAI with endpoint: {Endpoint}, deployment: {Deployment}", 
            azureOpenAIEndpoint, azureOpenAIDeployment);
    }
    else
    {
        logger.LogWarning("Azure OpenAI configuration not found, using fallback configuration");
        // Fallback configuration for development/testing
    }

    return kernelBuilder.Build();
});

// Configure HTTP clients with retry policies
builder.Services.AddHttpClient<IExternalServiceIntegration, ExternalServiceIntegration>()
    .AddPolicyHandler(GetRetryPolicy());

// Register repositories
builder.Services.AddScoped<IAgentSessionRepository, AgentSessionRepository>();
builder.Services.AddScoped<IAgentSkillRepository, AgentSkillRepository>();

// Register services
builder.Services.AddScoped<IAgentOrchestrationService, SemanticKernelOrchestrationService>();
builder.Services.AddScoped<ISemanticKernelService, SemanticKernelOrchestrationService>();
builder.Services.AddScoped<IExternalServiceIntegration, ExternalServiceIntegration>();
builder.Services.AddScoped<IAgentCacheService, AgentCacheService>();

// Configure health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AgentDbContext>()
    .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379")
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ?? "");

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure authentication and authorization (if needed)
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options => { /* Configure JWT */ });

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add middleware for exception handling
app.UseMiddleware<GlobalExceptionMiddleware>();

// Add authentication and authorization middleware
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

// Map health checks
app.MapHealthChecks("/health");

// Ensure database is created and migrated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AgentDbContext>();
    context.Database.EnsureCreated();
}

Log.Information("Copilot Agent Service started successfully");

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
            });
}

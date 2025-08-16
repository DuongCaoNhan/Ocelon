using AuditService.Application.Commands;
using AuditService.Domain.Events;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuditService.Infrastructure.Services;

/// <summary>
/// Background service that processes audit events from Azure Service Bus
/// Implements reliable message processing with error handling and dead letter queue support
/// </summary>
public class AuditEventProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuditEventProcessorService> _logger;
    private ServiceBusClient? _serviceBusClient;
    private ServiceBusProcessor? _processor;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuditEventProcessorService(
        IServiceProvider serviceProvider,
        ILogger<AuditEventProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            InitializeServiceBusProcessor();

            if (_processor != null)
            {
                _logger.LogInformation("Starting Audit Event Processor Service");
                await _processor.StartProcessingAsync(stoppingToken);

                // Keep the service running until cancellation is requested
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Audit Event Processor Service stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Audit Event Processor Service");
            throw;
        }
    }

    private void InitializeServiceBusProcessor()
    {
        using var scope = _serviceProvider.CreateScope();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var serviceBusNamespace = configuration["AzureServiceBus:Namespace"];
        var queueName = configuration["AzureServiceBus:AuditQueue"] ?? "audit-events";

        if (string.IsNullOrEmpty(serviceBusNamespace))
        {
            _logger.LogWarning("AzureServiceBus:Namespace not configured. Audit Event Processor will not start.");
            return;
        }

        // Create Service Bus client with Managed Identity
        var credential = new DefaultAzureCredential();
        _serviceBusClient = new ServiceBusClient(serviceBusNamespace, credential);

        // Configure processor options
        var processorOptions = new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 10, // Process up to 10 messages concurrently
            AutoCompleteMessages = false, // We'll handle completion manually
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
            PrefetchCount = 10 // Prefetch messages for better performance
        };

        _processor = _serviceBusClient.CreateProcessor(queueName, processorOptions);

        // Register message and error event handlers
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            _logger.LogDebug("Processing audit event message: {MessageId}", args.Message.MessageId);

            // Deserialize the audit event
            var messageBody = args.Message.Body.ToString();
            var auditEvent = JsonSerializer.Deserialize<AuditEvent>(messageBody, _jsonOptions);

            if (auditEvent == null)
            {
                _logger.LogWarning("Failed to deserialize audit event from message: {MessageId}", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "InvalidMessageFormat", "Failed to deserialize audit event");
                return;
            }

            // Process the audit event using MediatR
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(new ProcessAuditEventCommand(auditEvent), args.CancellationToken);

            // Complete the message
            await args.CompleteMessageAsync(args.Message);

            _logger.LogDebug("Successfully processed audit event: {EventId}", auditEvent.EventId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error for message: {MessageId}", args.Message.MessageId);
            await args.DeadLetterMessageAsync(args.Message, "DeserializationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing audit event message: {MessageId}", args.Message.MessageId);

            // Check delivery count to avoid infinite retry loops
            if (args.Message.DeliveryCount >= 3)
            {
                _logger.LogError("Message {MessageId} exceeded max delivery count, moving to dead letter queue", args.Message.MessageId);
                await args.DeadLetterMessageAsync(args.Message, "MaxRetryExceeded", ex.Message);
            }
            else
            {
                // Abandon the message to allow retry
                await args.AbandonMessageAsync(args.Message);
            }
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus error occurred. Source: {ErrorSource}, Entity Path: {EntityPath}", 
            args.ErrorSource, args.EntityPath);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Audit Event Processor Service");

        if (_processor != null)
        {
            await _processor.StopProcessingAsync(cancellationToken);
            await _processor.DisposeAsync();
        }

        if (_serviceBusClient != null)
        {
            await _serviceBusClient.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Audit Event Processor Service stopped");
    }

    public override void Dispose()
    {
        _processor?.DisposeAsync().AsTask().Wait();
        _serviceBusClient?.DisposeAsync().AsTask().Wait();
        base.Dispose();
    }
}

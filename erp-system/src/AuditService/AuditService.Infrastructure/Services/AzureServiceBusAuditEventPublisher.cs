using AuditService.Domain.Contracts;
using AuditService.Domain.Events;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AuditService.Infrastructure.Services;

/// <summary>
/// Azure Service Bus implementation of IAuditEventPublisher
/// Provides resilient, scalable audit event publishing
/// </summary>
public class AzureServiceBusAuditEventPublisher : IAuditEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _serviceBusClient;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<AzureServiceBusAuditEventPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    public AzureServiceBusAuditEventPublisher(
        IConfiguration configuration,
        ILogger<AzureServiceBusAuditEventPublisher> logger)
    {
        _logger = logger;

        // Get Service Bus configuration
        var serviceBusNamespace = configuration["AzureServiceBus:Namespace"] 
            ?? throw new InvalidOperationException("AzureServiceBus:Namespace configuration is required");
        var queueName = configuration["AzureServiceBus:AuditQueue"] ?? "audit-events";

        // Create Service Bus client with Managed Identity (recommended for Azure-hosted services)
        var credential = new DefaultAzureCredential();
        _serviceBusClient = new ServiceBusClient(serviceBusNamespace, credential);
        _sender = _serviceBusClient.CreateSender(queueName);

        // Configure JSON serialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AzureServiceBusAuditEventPublisher));

        try
        {
            var messageBody = JsonSerializer.Serialize(auditEvent, _jsonOptions);
            var message = new ServiceBusMessage(messageBody)
            {
                MessageId = auditEvent.EventId.ToString(),
                CorrelationId = auditEvent.CorrelationId,
                Subject = $"{auditEvent.ServiceName}.{auditEvent.ActionType}",
                ContentType = "application/json"
            };

            // Add message properties for routing and filtering
            message.ApplicationProperties.Add("ServiceName", auditEvent.ServiceName);
            message.ApplicationProperties.Add("ActionType", auditEvent.ActionType.ToString());
            message.ApplicationProperties.Add("Severity", auditEvent.Severity.ToString());
            message.ApplicationProperties.Add("EntityType", auditEvent.EntityType ?? string.Empty);
            message.ApplicationProperties.Add("UserId", auditEvent.UserId ?? string.Empty);
            message.ApplicationProperties.Add("TenantId", auditEvent.TenantId ?? string.Empty);

            // Set time to live based on severity
            message.TimeToLive = auditEvent.Severity switch
            {
                Domain.Enums.AuditSeverity.Critical => TimeSpan.FromDays(30),
                Domain.Enums.AuditSeverity.Error => TimeSpan.FromDays(7),
                Domain.Enums.AuditSeverity.Warning => TimeSpan.FromDays(3),
                _ => TimeSpan.FromDays(1)
            };

            await _sender.SendMessageAsync(message, cancellationToken);

            _logger.LogDebug("Successfully published audit event: {EventId} for service: {ServiceName}", 
                auditEvent.EventId, auditEvent.ServiceName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit event: {EventId}", auditEvent.EventId);
            throw;
        }
    }

    public async Task PublishBatchAsync(IEnumerable<AuditEvent> auditEvents, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AzureServiceBusAuditEventPublisher));

        var eventList = auditEvents.ToList();
        if (!eventList.Any())
        {
            return;
        }

        try
        {
            // Create message batch
            using var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken);

            foreach (var auditEvent in eventList)
            {
                var messageBody = JsonSerializer.Serialize(auditEvent, _jsonOptions);
                var message = new ServiceBusMessage(messageBody)
                {
                    MessageId = auditEvent.EventId.ToString(),
                    CorrelationId = auditEvent.CorrelationId,
                    Subject = $"{auditEvent.ServiceName}.{auditEvent.ActionType}",
                    ContentType = "application/json"
                };

                // Add message properties for routing and filtering
                message.ApplicationProperties.Add("ServiceName", auditEvent.ServiceName);
                message.ApplicationProperties.Add("ActionType", auditEvent.ActionType.ToString());
                message.ApplicationProperties.Add("Severity", auditEvent.Severity.ToString());
                message.ApplicationProperties.Add("EntityType", auditEvent.EntityType ?? string.Empty);
                message.ApplicationProperties.Add("UserId", auditEvent.UserId ?? string.Empty);
                message.ApplicationProperties.Add("TenantId", auditEvent.TenantId ?? string.Empty);

                // Set time to live based on severity
                message.TimeToLive = auditEvent.Severity switch
                {
                    Domain.Enums.AuditSeverity.Critical => TimeSpan.FromDays(30),
                    Domain.Enums.AuditSeverity.Error => TimeSpan.FromDays(7),
                    Domain.Enums.AuditSeverity.Warning => TimeSpan.FromDays(3),
                    _ => TimeSpan.FromDays(1)
                };

                // Try to add message to batch
                if (!messageBatch.TryAddMessage(message))
                {
                    // If the message is too large for the batch, send current batch and create a new one
                    if (messageBatch.Count > 0)
                    {
                        await _sender.SendMessagesAsync(messageBatch, cancellationToken);
                    }

                    // Send the current message individually since it's too large for batch
                    await _sender.SendMessageAsync(message, cancellationToken);
                }
            }

            // Send remaining messages in the batch
            if (messageBatch.Count > 0)
            {
                await _sender.SendMessagesAsync(messageBatch, cancellationToken);
            }

            _logger.LogDebug("Successfully published batch of {Count} audit events", eventList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch of {Count} audit events", eventList.Count);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            try
            {
                await _sender.DisposeAsync();
                await _serviceBusClient.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing AzureServiceBusAuditEventPublisher");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

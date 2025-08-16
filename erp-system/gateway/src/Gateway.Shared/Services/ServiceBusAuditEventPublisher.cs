using AuditService.Domain.Contracts;
using AuditService.Domain.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Gateway.Shared.Services;

public class ServiceBusOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string AuditQueueName { get; set; } = "audit-events";
}

public class ServiceBusAuditEventPublisher : IAuditEventPublisher
{
    private readonly ServiceBusClient? _serviceBusClient;
    private readonly ServiceBusSender? _sender;
    private readonly ILogger<ServiceBusAuditEventPublisher> _logger;
    private readonly ServiceBusOptions _options;

    public ServiceBusAuditEventPublisher(
        IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusAuditEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(_options.ConnectionString))
        {
            _logger.LogWarning("Service Bus connection string is not configured. Audit events will not be published.");
            return;
        }

        try
        {
            _serviceBusClient = new ServiceBusClient(_options.ConnectionString);
            _sender = _serviceBusClient.CreateSender(_options.AuditQueueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Service Bus client for audit logging");
        }
    }

    public async Task PublishAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        if (_sender == null)
        {
            _logger.LogWarning("Service Bus sender is not initialized. Skipping audit event publication.");
            return;
        }

        try
        {
            var messageBody = JsonSerializer.Serialize(auditEvent);
            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                MessageId = auditEvent.EventId.ToString(),
                Subject = auditEvent.ActionType.ToString(),
                CorrelationId = auditEvent.CorrelationId
            };

            // Add custom properties for better routing and filtering
            message.ApplicationProperties["ServiceName"] = auditEvent.ServiceName;
            message.ApplicationProperties["UserId"] = auditEvent.UserId ?? string.Empty;
            message.ApplicationProperties["Action"] = auditEvent.ActionType.ToString();
            message.ApplicationProperties["Result"] = auditEvent.Result.ToString();
            message.ApplicationProperties["Severity"] = auditEvent.Severity.ToString();

            await _sender.SendMessageAsync(message, cancellationToken);

            _logger.LogDebug("Audit event published successfully: {EventId}", auditEvent.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish audit event: {EventId}", auditEvent.EventId);
            throw;
        }
    }

    public async Task PublishBatchAsync(IEnumerable<AuditEvent> auditEvents, CancellationToken cancellationToken = default)
    {
        if (_sender == null)
        {
            _logger.LogWarning("Service Bus sender is not initialized. Skipping audit events publication.");
            return;
        }

        var events = auditEvents.ToList();
        if (!events.Any())
        {
            return;
        }

        try
        {
            var messages = events.Select(auditEvent =>
            {
                var messageBody = JsonSerializer.Serialize(auditEvent);
                var message = new ServiceBusMessage(messageBody)
                {
                    ContentType = "application/json",
                    MessageId = auditEvent.EventId.ToString(),
                    Subject = auditEvent.ActionType.ToString(),
                    CorrelationId = auditEvent.CorrelationId
                };

                // Add custom properties for better routing and filtering
                message.ApplicationProperties["ServiceName"] = auditEvent.ServiceName;
                message.ApplicationProperties["UserId"] = auditEvent.UserId ?? string.Empty;
                message.ApplicationProperties["Action"] = auditEvent.ActionType.ToString();
                message.ApplicationProperties["Result"] = auditEvent.Result.ToString();
                message.ApplicationProperties["Severity"] = auditEvent.Severity.ToString();

                return message;
            }).ToList();

            await _sender.SendMessagesAsync(messages, cancellationToken);

            _logger.LogDebug("Batch of {Count} audit events published successfully", events.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish batch of {Count} audit events", events.Count);
            throw;
        }
    }

    public void Dispose()
    {
        _sender?.DisposeAsync().GetAwaiter().GetResult();
        _serviceBusClient?.DisposeAsync().GetAwaiter().GetResult();
    }
}

namespace AuditService.Domain.Contracts;

/// <summary>
/// Service interface for publishing audit events to message broker
/// Provides decoupled audit logging capabilities
/// </summary>
public interface IAuditEventPublisher
{
    /// <summary>
    /// Publishes a single audit event asynchronously
    /// </summary>
    /// <param name="auditEvent">The audit event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishAsync(Events.AuditEvent auditEvent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Publishes multiple audit events as a batch asynchronously
    /// </summary>
    /// <param name="auditEvents">The audit events to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the async operation</returns>
    Task PublishBatchAsync(IEnumerable<Events.AuditEvent> auditEvents, CancellationToken cancellationToken = default);
}

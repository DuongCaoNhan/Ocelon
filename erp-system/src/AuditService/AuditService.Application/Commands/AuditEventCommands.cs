using AuditService.Domain.Events;
using MediatR;

namespace AuditService.Application.Commands;

/// <summary>
/// Command to process a single audit event
/// </summary>
public class ProcessAuditEventCommand : IRequest
{
    public AuditEvent AuditEvent { get; }
    
    public ProcessAuditEventCommand(AuditEvent auditEvent)
    {
        AuditEvent = auditEvent;
    }
}

/// <summary>
/// Command to process multiple audit events in batch
/// </summary>
public class ProcessAuditEventBatchCommand : IRequest
{
    public IEnumerable<AuditEvent> AuditEvents { get; }
    
    public ProcessAuditEventBatchCommand(IEnumerable<AuditEvent> auditEvents)
    {
        AuditEvents = auditEvents;
    }
}

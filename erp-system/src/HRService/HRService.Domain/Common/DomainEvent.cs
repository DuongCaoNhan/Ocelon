using MediatR;

namespace HRService.Domain.Common;

public abstract class DomainEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid EventId { get; } = Guid.NewGuid();
}

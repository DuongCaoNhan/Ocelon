namespace CopilotAgentService.Domain.Common
{
    /// <summary>
    /// Base class for domain events to support event-driven architecture
    /// </summary>
    public abstract class DomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
        public string EventType => GetType().Name;
    }

    /// <summary>
    /// Interface for entities that can raise domain events
    /// </summary>
    public interface IHasDomainEvents
    {
        IReadOnlyCollection<DomainEvent> DomainEvents { get; }
        void AddDomainEvent(DomainEvent domainEvent);
        void RemoveDomainEvent(DomainEvent domainEvent);
        void ClearDomainEvents();
    }
}

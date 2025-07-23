namespace AIAgentService.Domain.Common
{
    /// <summary>
    /// Interface for domain events
    /// </summary>
    public interface IDomainEvent
    {
        Guid Id { get; }
        DateTime OccurredOn { get; }
        string EventType { get; }
    }

    /// <summary>
    /// Base class for domain events to support event-driven architecture
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
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

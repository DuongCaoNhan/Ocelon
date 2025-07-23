using AIAgentService.Domain.Common;
using AIAgentService.Domain.Events;

namespace AIAgentService.Domain.Entities
{
    /// <summary>
    /// Represents an AI agent conversation session
    /// </summary>
    public class AgentSession : BaseEntity, IHasDomainEvents
    {
        private readonly List<DomainEvent> _domainEvents = new();

        public string UserId { get; private set; } = string.Empty;
        public string SessionName { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public AgentSessionStatus Status { get; private set; }
        public string? Context { get; private set; }
        public DateTime? EndedAt { get; private set; }
        
        private readonly List<AgentMessage> _messages = new();
        public IReadOnlyCollection<AgentMessage> Messages => _messages.AsReadOnly();

        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private AgentSession() { } // For EF Core

        public AgentSession(string userId, string sessionName, string? description = null, string? context = null)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            SessionName = sessionName ?? throw new ArgumentNullException(nameof(sessionName));
            Description = description;
            Context = context;
            Status = AgentSessionStatus.Active;
            
            AddDomainEvent(new AgentSessionStartedEvent(Id, UserId));
        }

        public void AddMessage(string content, MessageType messageType, string? metadata = null)
        {
            if (Status != AgentSessionStatus.Active)
                throw new InvalidOperationException("Cannot add messages to inactive session");

            var message = new AgentMessage(Id, content, messageType, metadata);
            _messages.Add(message);
            
            AddDomainEvent(new MessageAddedToSessionEvent(Id, message.Id, messageType));
        }

        public void EndSession()
        {
            if (Status == AgentSessionStatus.Ended)
                return;

            Status = AgentSessionStatus.Ended;
            EndedAt = DateTime.UtcNow;
            
            AddDomainEvent(new AgentSessionEndedEvent(Id, UserId));
        }

        public void UpdateContext(string context)
        {
            Context = context;
            SetUpdatedInfo();
        }

        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }

    public enum AgentSessionStatus
    {
        Active,
        Paused,
        Ended
    }
}

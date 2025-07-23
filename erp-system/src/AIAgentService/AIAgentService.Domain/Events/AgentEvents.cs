using AIAgentService.Domain.Common;
using AIAgentService.Domain.Entities;

namespace AIAgentService.Domain.Events
{
    public class AgentSessionStartedEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }

        public AgentSessionStartedEvent(Guid sessionId, string userId)
        {
            SessionId = sessionId;
            UserId = userId;
        }
    }

    public class AgentSessionEndedEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public string UserId { get; }

        public AgentSessionEndedEvent(Guid sessionId, string userId)
        {
            SessionId = sessionId;
            UserId = userId;
        }
    }

    public class MessageAddedToSessionEvent : DomainEvent
    {
        public Guid SessionId { get; }
        public Guid MessageId { get; }
        public MessageType MessageType { get; }

        public MessageAddedToSessionEvent(Guid sessionId, Guid messageId, MessageType messageType)
        {
            SessionId = sessionId;
            MessageId = messageId;
            MessageType = messageType;
        }
    }
}

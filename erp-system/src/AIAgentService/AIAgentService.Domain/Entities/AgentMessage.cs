using AIAgentService.Domain.Common;

namespace AIAgentService.Domain.Entities
{
    /// <summary>
    /// Represents a message in an agent conversation
    /// </summary>
    public class AgentMessage : BaseEntity
    {
        public Guid SessionId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public MessageType MessageType { get; private set; }
        public string? Metadata { get; private set; }
        public int SequenceNumber { get; private set; }

        private AgentMessage() { } // For EF Core

        public AgentMessage(Guid sessionId, string content, MessageType messageType, string? metadata = null)
        {
            SessionId = sessionId;
            Content = content ?? throw new ArgumentNullException(nameof(content));
            MessageType = messageType;
            Metadata = metadata;
            SequenceNumber = 0; // Will be set by the repository based on session message count
        }

        public void SetSequenceNumber(int sequenceNumber)
        {
            SequenceNumber = sequenceNumber;
        }
    }

    public enum MessageType
    {
        User,
        Agent,
        System,
        Tool,
        Error
    }
}

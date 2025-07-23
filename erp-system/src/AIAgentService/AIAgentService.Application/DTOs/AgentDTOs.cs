namespace AIAgentService.Application.DTOs
{
    public class AgentSessionDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Context { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<AgentMessageDto> Messages { get; set; } = new();
    }

    public class AgentMessageDto
    {
        public Guid Id { get; set; }
        public Guid SessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public string? Metadata { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AgentSkillDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string SkillType { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}

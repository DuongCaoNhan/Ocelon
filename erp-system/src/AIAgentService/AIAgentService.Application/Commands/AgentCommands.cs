using MediatR;
using AIAgentService.Application.DTOs;

namespace AIAgentService.Application.Commands
{
    // Create Agent Session
    public class CreateAgentSessionCommand : IRequest<AgentSessionDto>
    {
        public string UserId { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Context { get; set; }
    }

    // End Agent Session
    public class EndAgentSessionCommand : IRequest<bool>
    {
        public Guid SessionId { get; set; }
    }

    // Send Message to Agent
    public class SendMessageToAgentCommand : IRequest<AgentMessageDto>
    {
        public Guid SessionId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? Metadata { get; set; }
    }

    // Create Agent Skill
    public class CreateAgentSkillCommand : IRequest<AgentSkillDto>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string SkillType { get; set; } = string.Empty;
        public string Configuration { get; set; } = string.Empty;
        public string? Version { get; set; }
    }

    // Update Agent Skill
    public class UpdateAgentSkillCommand : IRequest<AgentSkillDto>
    {
        public Guid Id { get; set; }
        public string? Configuration { get; set; }
        public string? Version { get; set; }
        public bool? IsActive { get; set; }
    }
}

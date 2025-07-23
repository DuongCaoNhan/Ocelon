using MediatR;
using AIAgentService.Application.DTOs;

namespace AIAgentService.Application.Queries
{
    // Get Agent Session by ID
    public class GetAgentSessionByIdQuery : IRequest<AgentSessionDto?>
    {
        public Guid SessionId { get; set; }
    }

    // Get Agent Sessions by User
    public class GetAgentSessionsByUserQuery : IRequest<IEnumerable<AgentSessionDto>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    // Get Active Agent Sessions
    public class GetActiveAgentSessionsQuery : IRequest<IEnumerable<AgentSessionDto>>
    {
    }

    // Get Agent Skills
    public class GetAgentSkillsQuery : IRequest<IEnumerable<AgentSkillDto>>
    {
        public bool? IsActive { get; set; }
        public string? ServiceName { get; set; }
        public string? SkillType { get; set; }
    }

    // Get Agent Skill by ID
    public class GetAgentSkillByIdQuery : IRequest<AgentSkillDto?>
    {
        public Guid SkillId { get; set; }
    }

    // Get Available Skills for Agent
    public class GetAvailableSkillsQuery : IRequest<IEnumerable<string>>
    {
    }
}

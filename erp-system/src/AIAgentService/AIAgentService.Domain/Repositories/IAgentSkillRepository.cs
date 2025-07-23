using AIAgentService.Domain.Entities;

namespace AIAgentService.Domain.Repositories
{
    /// <summary>
    /// Repository interface for AgentSkill entities
    /// </summary>
    public interface IAgentSkillRepository
    {
        Task<AgentSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AgentSkill?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<AgentSkill>> GetActiveSkillsAsync(CancellationToken cancellationToken = default);
        Task<IEnumerable<AgentSkill>> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default);
        Task<IEnumerable<AgentSkill>> GetBySkillTypeAsync(string skillType, CancellationToken cancellationToken = default);
        Task<AgentSkill> AddAsync(AgentSkill skill, CancellationToken cancellationToken = default);
        Task UpdateAsync(AgentSkill skill, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

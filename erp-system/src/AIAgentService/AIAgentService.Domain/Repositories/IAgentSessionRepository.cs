using AIAgentService.Domain.Entities;

namespace AIAgentService.Domain.Repositories
{
    /// <summary>
    /// Repository interface for AgentSession aggregate root
    /// </summary>
    public interface IAgentSessionRepository
    {
        Task<AgentSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<AgentSession>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<AgentSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);
        Task<AgentSession> AddAsync(AgentSession session, CancellationToken cancellationToken = default);
        Task UpdateAsync(AgentSession session, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}

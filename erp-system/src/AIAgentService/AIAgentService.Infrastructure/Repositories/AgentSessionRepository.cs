using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AIAgentService.Domain.Entities;
using AIAgentService.Domain.Repositories;
using AIAgentService.Infrastructure.Data;

namespace AIAgentService.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework implementation of IAgentSessionRepository
    /// Provides data access for AgentSession aggregate root
    /// </summary>
    public class AgentSessionRepository : IAgentSessionRepository
    {
        private readonly AgentDbContext _context;
        private readonly ILogger<AgentSessionRepository> _logger;

        public AgentSessionRepository(AgentDbContext context, ILogger<AgentSessionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AgentSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent session {SessionId}", id);

                return await _context.AgentSessions
                    .Include(s => s.Messages.OrderBy(m => m.SequenceNumber))
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent session {SessionId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AgentSession>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent sessions for user {UserId}", userId);

                return await _context.AgentSessions
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent sessions for user {UserId}", userId);
                throw;
            }
        }

        public async Task<IEnumerable<AgentSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving active agent sessions");

                return await _context.AgentSessions
                    .Where(s => s.Status == AgentSessionStatus.Active)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active agent sessions");
                throw;
            }
        }

        public async Task<AgentSession> AddAsync(AgentSession session, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Adding new agent session for user {UserId}", session.UserId);

                _context.AgentSessions.Add(session);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Agent session {SessionId} added successfully", session.Id);
                return session;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agent session for user {UserId}", session.UserId);
                throw;
            }
        }

        public async Task UpdateAsync(AgentSession session, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Updating agent session {SessionId}", session.Id);

                // Set sequence numbers for new messages
                var existingMessageCount = await _context.AgentMessages
                    .Where(m => m.SessionId == session.Id)
                    .CountAsync(cancellationToken);

                var newMessages = session.Messages.Where(m => m.SequenceNumber == 0).ToList();
                for (int i = 0; i < newMessages.Count; i++)
                {
                    newMessages[i].SetSequenceNumber(existingMessageCount + i + 1);
                }

                _context.AgentSessions.Update(session);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Agent session {SessionId} updated successfully", session.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent session {SessionId}", session.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Deleting agent session {SessionId}", id);

                var session = await _context.AgentSessions.FindAsync(new object[] { id }, cancellationToken);
                if (session != null)
                {
                    _context.AgentSessions.Remove(session);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Agent session {SessionId} deleted successfully", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agent session {SessionId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.AgentSessions.AnyAsync(s => s.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of agent session {SessionId}", id);
                throw;
            }
        }
    }
}

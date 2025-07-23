using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AIAgentService.Domain.Entities;
using AIAgentService.Domain.Repositories;
using AIAgentService.Infrastructure.Data;

namespace AIAgentService.Infrastructure.Repositories
{
    /// <summary>
    /// Entity Framework implementation of IAgentSkillRepository
    /// Provides data access for AgentSkill entities
    /// </summary>
    public class AgentSkillRepository : IAgentSkillRepository
    {
        private readonly AgentDbContext _context;
        private readonly ILogger<AgentSkillRepository> _logger;

        public AgentSkillRepository(AgentDbContext context, ILogger<AgentSkillRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AgentSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent skill {SkillId}", id);

                return await _context.AgentSkills
                    .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skill {SkillId}", id);
                throw;
            }
        }

        public async Task<AgentSkill?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent skill by name {SkillName}", name);

                return await _context.AgentSkills
                    .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skill by name {SkillName}", name);
                throw;
            }
        }

        public async Task<IEnumerable<AgentSkill>> GetActiveSkillsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving active agent skills");

                return await _context.AgentSkills
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active agent skills");
                throw;
            }
        }

        public async Task<IEnumerable<AgentSkill>> GetByServiceNameAsync(string serviceName, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent skills for service {ServiceName}", serviceName);

                return await _context.AgentSkills
                    .Where(s => s.ServiceName == serviceName)
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skills for service {ServiceName}", serviceName);
                throw;
            }
        }

        public async Task<IEnumerable<AgentSkill>> GetBySkillTypeAsync(string skillType, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Retrieving agent skills by type {SkillType}", skillType);

                return await _context.AgentSkills
                    .Where(s => s.SkillType == skillType)
                    .OrderBy(s => s.Name)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent skills by type {SkillType}", skillType);
                throw;
            }
        }

        public async Task<AgentSkill> AddAsync(AgentSkill skill, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Adding new agent skill {SkillName}", skill.Name);

                _context.AgentSkills.Add(skill);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Agent skill {SkillName} added successfully", skill.Name);
                return skill;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding agent skill {SkillName}", skill.Name);
                throw;
            }
        }

        public async Task UpdateAsync(AgentSkill skill, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Updating agent skill {SkillName}", skill.Name);

                _context.AgentSkills.Update(skill);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogDebug("Agent skill {SkillName} updated successfully", skill.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent skill {SkillName}", skill.Name);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Deleting agent skill {SkillId}", id);

                var skill = await _context.AgentSkills.FindAsync(new object[] { id }, cancellationToken);
                if (skill != null)
                {
                    _context.AgentSkills.Remove(skill);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Agent skill {SkillId} deleted successfully", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting agent skill {SkillId}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.AgentSkills.AnyAsync(s => s.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of agent skill {SkillId}", id);
                throw;
            }
        }
    }
}

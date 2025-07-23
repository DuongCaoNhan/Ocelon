using Microsoft.EntityFrameworkCore;
using AIAgentService.Domain.Entities;
using AIAgentService.Domain.Common;
using AIAgentService.Infrastructure.Data.Configurations;

namespace AIAgentService.Infrastructure.Data
{
    /// <summary>
    /// Entity Framework DbContext for the Copilot Agent Service
    /// Implements Unit of Work pattern and provides data access
    /// </summary>
    public class AgentDbContext : DbContext
    {
        public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
        {
        }

        public DbSet<AgentSession> AgentSessions { get; set; } = null!;
        public DbSet<AgentMessage> AgentMessages { get; set; } = null!;
        public DbSet<AgentSkill> AgentSkills { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations
            modelBuilder.ApplyConfiguration(new AgentSessionConfiguration());
            modelBuilder.ApplyConfiguration(new AgentMessageConfiguration());
            modelBuilder.ApplyConfiguration(new AgentSkillConfiguration());

            // Global query filters for soft delete if needed in the future
            // modelBuilder.Entity<AgentSession>().HasQueryFilter(e => !e.IsDeleted);
        }

        /// <summary>
        /// Override SaveChanges to automatically set audit fields
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Override SaveChanges to automatically set audit fields
        /// </summary>
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    // CreatedAt is set in the constructor of BaseEntity
                    // CreatedBy could be set here if we have access to the current user context
                }
                else if (entry.State == EntityState.Modified)
                {
                    // UpdatedAt and UpdatedBy are handled in the domain entities
                    entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
                }
            }
        }
    }
}

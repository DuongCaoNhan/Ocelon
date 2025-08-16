using AuditService.Domain.Entities;
using AuditService.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AuditService.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for audit logging
/// Optimized for high-volume write operations and efficient querying
/// </summary>
public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Audit logs dataset
    /// </summary>
    public DbSet<AuditLog> AuditLogs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());

        // Configure schema
        modelBuilder.HasDefaultSchema("audit");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This will be overridden by DI configuration
            // Only here for design-time tooling
            optionsBuilder.UseSqlServer();
        }

        // Performance optimizations for audit logging
        optionsBuilder.EnableSensitiveDataLogging(false); // Security: never log sensitive data
        optionsBuilder.EnableDetailedErrors(false); // Performance: disable in production
        
        // Configure for write-heavy workload
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        
        base.OnConfiguring(optionsBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Audit logs are immutable - only allow inserts
        var entries = ChangeTracker.Entries<AuditLog>();
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                throw new InvalidOperationException("Audit logs are immutable and cannot be modified or deleted.");
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        // Audit logs are immutable - only allow inserts
        var entries = ChangeTracker.Entries<AuditLog>();
        
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                throw new InvalidOperationException("Audit logs are immutable and cannot be modified or deleted.");
            }
        }

        return base.SaveChanges();
    }
}

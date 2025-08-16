using AuditService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditService.Infrastructure.Data.Configurations;

/// <summary>
/// Entity Framework configuration for AuditLog entity
/// Defines database schema, indexes, and constraints for optimal performance
/// </summary>
public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table configuration
        builder.ToTable("AuditLogs");

        // Primary key
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .ValueGeneratedNever(); // We generate GUIDs in the domain

        // Required fields
        builder.Property(x => x.CorrelationId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ServiceName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Timestamp)
            .IsRequired();

        // Optional string fields with max lengths
        builder.Property(x => x.UserId)
            .HasMaxLength(100);

        builder.Property(x => x.UserDisplayName)
            .HasMaxLength(200);

        builder.Property(x => x.TenantId)
            .HasMaxLength(100);

        builder.Property(x => x.EntityType)
            .HasMaxLength(100);

        builder.Property(x => x.EntityId)
            .HasMaxLength(100);

        builder.Property(x => x.EntityName)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(x => x.UserAgent)
            .HasMaxLength(500);

        builder.Property(x => x.Source)
            .HasMaxLength(100);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.Tags)
            .HasMaxLength(1000);

        builder.Property(x => x.RetentionCategory)
            .HasMaxLength(50);

        builder.Property(x => x.DataHash)
            .HasMaxLength(128);

        builder.Property(x => x.SchemaVersion)
            .HasMaxLength(20)
            .HasDefaultValue("1.0");

        // Large text fields stored as nvarchar(max)
        builder.Property(x => x.OriginalData)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.NewData)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.StackTrace)
            .HasColumnType("nvarchar(max)");

        // Enum conversions
        builder.Property(x => x.ActionType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Result)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(50);

        // Indexes for optimal query performance
        
        // Primary timestamp index (clustered) - most queries filter by time
        builder.HasIndex(x => x.Timestamp)
            .HasDatabaseName("IX_AuditLogs_Timestamp");

        // Correlation ID index for request tracing
        builder.HasIndex(x => x.CorrelationId)
            .HasDatabaseName("IX_AuditLogs_CorrelationId");

        // User tracking index
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("IX_AuditLogs_UserId");

        // Service name index
        builder.HasIndex(x => x.ServiceName)
            .HasDatabaseName("IX_AuditLogs_ServiceName");

        // Entity tracking composite index
        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("IX_AuditLogs_Entity");

        // Action type index for filtering
        builder.HasIndex(x => x.ActionType)
            .HasDatabaseName("IX_AuditLogs_ActionType");

        // Severity index for monitoring
        builder.HasIndex(x => x.Severity)
            .HasDatabaseName("IX_AuditLogs_Severity");

        // Retention management index
        builder.HasIndex(x => x.ExpiryDate)
            .HasDatabaseName("IX_AuditLogs_ExpiryDate");

        // Composite index for common query patterns
        builder.HasIndex(x => new { x.ServiceName, x.Timestamp })
            .HasDatabaseName("IX_AuditLogs_Service_Timestamp");

        builder.HasIndex(x => new { x.UserId, x.Timestamp })
            .HasDatabaseName("IX_AuditLogs_User_Timestamp");

        // IP address index for security analysis
        builder.HasIndex(x => x.IpAddress)
            .HasDatabaseName("IX_AuditLogs_IpAddress");

        // Tenant isolation index
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("IX_AuditLogs_TenantId");
    }
}

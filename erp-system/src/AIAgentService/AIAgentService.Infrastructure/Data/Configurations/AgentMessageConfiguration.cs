using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AIAgentService.Domain.Entities;

namespace AIAgentService.Infrastructure.Data.Configurations
{
    public class AgentMessageConfiguration : IEntityTypeConfiguration<AgentMessage>
    {
        public void Configure(EntityTypeBuilder<AgentMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.SessionId)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(10000);

            builder.Property(x => x.MessageType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.Metadata)
                .HasMaxLength(2000);

            builder.Property(x => x.SequenceNumber)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(x => x.SessionId);
            builder.HasIndex(x => x.MessageType);
            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => new { x.SessionId, x.SequenceNumber })
                .IsUnique();

            builder.ToTable("AgentMessages");
        }
    }

    public class AgentSkillConfiguration : IEntityTypeConfiguration<AgentSkill>
    {
        public void Configure(EntityTypeBuilder<AgentSkill> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ServiceName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SkillType)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Configuration)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .IsRequired();

            builder.Property(x => x.Version)
                .HasMaxLength(20);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(100);

            // Indexes
            builder.HasIndex(x => x.Name)
                .IsUnique();
            builder.HasIndex(x => x.ServiceName);
            builder.HasIndex(x => x.SkillType);
            builder.HasIndex(x => x.IsActive);

            builder.ToTable("AgentSkills");
        }
    }
}

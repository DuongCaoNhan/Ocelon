using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AIAgentService.Domain.Entities;

namespace AIAgentService.Infrastructure.Data.Configurations
{
    public class AgentSessionConfiguration : IEntityTypeConfiguration<AgentSession>
    {
        public void Configure(EntityTypeBuilder<AgentSession> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.SessionName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(x => x.Context)
                .HasMaxLength(5000);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);
            
            builder.Property(x => x.EndedAt);

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(100);

            // Configure relationships
            builder.HasMany<AgentMessage>()
                .WithOne()
                .HasForeignKey(m => m.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.Status);
            builder.HasIndex(x => x.CreatedAt);

            // Ignore domain events property
            builder.Ignore(x => x.DomainEvents);

            builder.ToTable("AgentSessions");
        }
    }
}

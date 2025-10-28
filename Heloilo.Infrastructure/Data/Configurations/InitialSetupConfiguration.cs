using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class InitialSetupConfiguration : IEntityTypeConfiguration<InitialSetup>
{
    public void Configure(EntityTypeBuilder<InitialSetup> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(i => i.IsSkipped)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(i => new { i.RelationshipId, i.UserId })
            .IsUnique();

        builder.HasIndex(i => i.IsCompleted);

        // Relationships
        builder.HasOne(i => i.Relationship)
            .WithMany(r => r.InitialSetups)
            .HasForeignKey(i => i.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.User)
            .WithMany(u => u.InitialSetups)
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

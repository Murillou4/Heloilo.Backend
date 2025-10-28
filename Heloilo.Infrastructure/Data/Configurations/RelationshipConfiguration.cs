using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class RelationshipConfiguration : IEntityTypeConfiguration<Relationship>
{
    public void Configure(EntityTypeBuilder<Relationship> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MetLocation)
            .HasMaxLength(255);

        builder.Property(r => r.CelebrationType)
            .HasConversion<string>()
            .HasDefaultValue(CelebrationType.Annual);

        // Required semantics handled by relationships

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Relationship_Users_Different", "User1Id != User2Id"));

        // Indexes
        builder.HasIndex(r => new { r.User1Id, r.User2Id, r.IsActive })
            .IsUnique()
            .HasFilter("IsActive = 1");

        builder.HasIndex(r => new { r.IsActive, r.DeletedAt });

        builder.HasIndex(r => r.RelationshipStartDate);

        // Relationships
        builder.HasOne(r => r.User1)
            .WithMany(u => u.RelationshipsAsUser1)
            .HasForeignKey(r => r.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.User2)
            .WithMany(u => u.RelationshipsAsUser2)
            .HasForeignKey(r => r.User2Id)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

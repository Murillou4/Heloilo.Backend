using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class MemoryConfiguration : IEntityTypeConfiguration<Memory>
{
    public void Configure(EntityTypeBuilder<Memory> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Description)
            .HasMaxLength(2000);

        builder.Property(m => m.MemoryDate)
            .IsRequired();

        // Required properties moved from DataAnnotations
        builder.Property(m => m.Title)
            .IsRequired();

        // Indexes
        builder.HasIndex(m => new { m.RelationshipId, m.MemoryDate });
        builder.HasIndex(m => m.DeletedAt);

        // Relationships
        builder.HasOne(m => m.Relationship)
            .WithMany(r => r.Memories)
            .HasForeignKey(m => m.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



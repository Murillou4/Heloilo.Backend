using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class StoryPageConfiguration : IEntityTypeConfiguration<StoryPage>
{
    public void Configure(EntityTypeBuilder<StoryPage> builder)
    {
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(sp => sp.Content)
            .HasMaxLength(5000);

        builder.Property(sp => sp.ImageBlob)
            .HasColumnType("BLOB");

        // Indexes
        builder.HasIndex(sp => new { sp.RelationshipId, sp.PageNumber })
            .IsUnique();

        builder.HasIndex(sp => new { sp.RelationshipId, sp.PageDate });

        // Relationships
        builder.HasOne(sp => sp.Relationship)
            .WithMany(r => r.StoryPages)
            .HasForeignKey(sp => sp.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        // Required properties set via Fluent API
        builder.Property(sp => sp.PageNumber)
            .IsRequired();

        builder.Property(sp => sp.PageDate)
            .IsRequired();
    }
}

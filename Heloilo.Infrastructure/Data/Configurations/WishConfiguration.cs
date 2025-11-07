using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class WishConfiguration : IEntityTypeConfiguration<Wish>
{
    public void Configure(EntityTypeBuilder<Wish> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.Description)
            .HasMaxLength(2000);

        builder.Property(w => w.LinkUrl)
            .HasMaxLength(1000);

        builder.Property(w => w.ImportanceLevel)
            .HasDefaultValue(3);

        builder.Property(w => w.Status)
            .HasConversion<int>()
            .HasDefaultValue(WishStatus.Pending);

        builder.Property(w => w.FulfilledAt);

        builder.Property(w => w.ImageBlob)
            .HasColumnType("BLOB");

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Wish_ImportanceLevel", "ImportanceLevel >= 1 AND ImportanceLevel <= 5"));

        // Required properties moved from DataAnnotations
        builder.Property(w => w.Title)
            .IsRequired();

        // Indexes
        builder.HasIndex(w => new { w.RelationshipId, w.DeletedAt });
        builder.HasIndex(w => new { w.UserId, w.CreatedAt });
        builder.HasIndex(w => w.CategoryId);
        builder.HasIndex(w => w.ImportanceLevel);
        builder.HasIndex(w => w.Status);
        builder.HasIndex(w => new { w.RelationshipId, w.Status, w.DeletedAt });

        // Relationships
        builder.HasOne(w => w.User)
            .WithMany(u => u.Wishes)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Relationship)
            .WithMany(r => r.Wishes)
            .HasForeignKey(w => w.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(w => w.Category)
            .WithMany(c => c.Wishes)
            .HasForeignKey(w => w.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

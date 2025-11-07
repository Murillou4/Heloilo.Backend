using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Heloilo.Infrastructure.Data.Configurations;

public class SharedContentConfiguration : IEntityTypeConfiguration<SharedContent>
{
    public void Configure(EntityTypeBuilder<SharedContent> builder)
    {
        builder.ToTable("shared_contents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.RelationshipId)
            .HasColumnName("relationship_id")
            .IsRequired();

        builder.Property(e => e.ContentType)
            .HasColumnName("content_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(e => e.ContentId)
            .HasColumnName("content_id")
            .IsRequired();

        builder.Property(e => e.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(e => e.IsRevoked)
            .HasColumnName("is_revoked")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(e => e.AccessCount)
            .HasColumnName("access_count")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.LastAccessedAt)
            .HasColumnName("last_accessed_at");

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(e => e.Relationship)
            .WithMany()
            .HasForeignKey(e => e.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(e => e.Token)
            .IsUnique();

        builder.HasIndex(e => new { e.RelationshipId, e.ContentType, e.ContentId });
        builder.HasIndex(e => e.ExpiresAt);
        builder.HasIndex(e => new { e.Token, e.IsRevoked, e.ExpiresAt });
    }
}


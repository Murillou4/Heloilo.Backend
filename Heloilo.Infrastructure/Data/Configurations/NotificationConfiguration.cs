using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(n => n.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(n => n.NotificationType)
            .HasConversion<string>();

        builder.Property(n => n.IsRead)
            .HasDefaultValue(false);

        builder.Property(n => n.SentAt)
            .HasDefaultValueSql("datetime('now')");

        // Indexes
        builder.HasIndex(n => new { n.RelationshipId, n.IsRead });
        builder.HasIndex(n => new { n.UserId, n.SentAt });

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Relationship)
            .WithMany(r => r.Notifications)
            .HasForeignKey(n => n.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



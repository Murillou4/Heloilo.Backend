using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class ReminderConfiguration : IEntityTypeConfiguration<Reminder>
{
    public void Configure(EntityTypeBuilder<Reminder> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.RecurrencePattern)
            .HasMaxLength(50);

        builder.Property(r => r.IsRecurring)
            .HasDefaultValue(false);

        builder.Property(r => r.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(r => r.IsActive)
            .HasDefaultValue(true);

        // Indexes
        builder.HasIndex(r => new { r.UserId, r.ReminderDate });
        builder.HasIndex(r => new { r.IsActive, r.ReminderDate });
        builder.HasIndex(r => r.RelationshipId);

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Relationship)
            .WithMany()
            .HasForeignKey(r => r.RelationshipId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}


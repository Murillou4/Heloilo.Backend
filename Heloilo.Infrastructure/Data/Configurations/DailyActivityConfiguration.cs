using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class DailyActivityConfiguration : IEntityTypeConfiguration<DailyActivity>
{
    public void Configure(EntityTypeBuilder<DailyActivity> builder)
    {
        builder.HasKey(da => da.Id);

        builder.Property(da => da.Title)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(da => da.Description)
            .HasMaxLength(2000);

        builder.Property(da => da.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(da => da.ActivityDate)
            .IsRequired();

        builder.Property(da => da.RecurrenceType)
            .HasConversion<int>()
            .HasDefaultValue(RecurrenceType.None);

        builder.Property(da => da.RecurrenceParentId);

        builder.Property(da => da.RecurrenceEndDate);

        // Indexes
        builder.HasIndex(da => new { da.UserId, da.ActivityDate });
        builder.HasIndex(da => new { da.UserId, da.IsCompleted });
        builder.HasIndex(da => da.DeletedAt);
        builder.HasIndex(da => da.RecurrenceParentId);
        builder.HasIndex(da => da.RecurrenceType);

        // Relationships
        builder.HasOne(da => da.User)
            .WithMany(u => u.DailyActivities)
            .HasForeignKey(da => da.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



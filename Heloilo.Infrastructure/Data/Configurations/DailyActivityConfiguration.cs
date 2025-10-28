using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

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

        // Indexes
        builder.HasIndex(da => new { da.UserId, da.ActivityDate });
        builder.HasIndex(da => new { da.UserId, da.IsCompleted });
        builder.HasIndex(da => da.DeletedAt);

        // Relationships
        builder.HasOne(da => da.User)
            .WithMany(u => u.DailyActivities)
            .HasForeignKey(da => da.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



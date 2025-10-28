using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.HasKey(np => np.Id);

        builder.Property(np => np.NotificationType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(np => np.IsEnabled)
            .HasDefaultValue(true);

        builder.Property(np => np.IntensityLevel)
            .IsRequired()
            .HasConversion<string>()
            .HasDefaultValue(IntensityLevel.Normal);

        // Indexes
        builder.HasIndex(np => new { np.UserId, np.NotificationType })
            .IsUnique();

        // Relationships
        builder.HasOne(np => np.User)
            .WithMany(u => u.NotificationPreferences)
            .HasForeignKey(np => np.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

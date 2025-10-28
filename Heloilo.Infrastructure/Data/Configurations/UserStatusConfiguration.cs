using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class UserStatusConfiguration : IEntityTypeConfiguration<UserStatus>
{
    public void Configure(EntityTypeBuilder<UserStatus> builder)
    {
        builder.HasKey(us => us.Id);

        builder.Property(us => us.CurrentStatus)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(us => us.StatusUpdatedAt)
            .HasDefaultValueSql("datetime('now')");

        // Indexes
        builder.HasIndex(us => us.UserId)
            .IsUnique();

        builder.HasIndex(us => us.StatusUpdatedAt);

        // Relationships
        builder.HasOne(us => us.User)
            .WithOne(u => u.UserStatus)
            .HasForeignKey<UserStatus>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

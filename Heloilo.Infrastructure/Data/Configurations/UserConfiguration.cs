using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;

namespace Heloilo.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Nickname)
            .HasMaxLength(50);

        builder.Property(u => u.ThemeColor)
            .HasMaxLength(7)
            .HasDefaultValue("#FF6B9D");

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.EmailVerified)
            .HasDefaultValue(false);

        builder.Property(u => u.ProfilePhotoBlob)
            .HasColumnType("BLOB");

        builder.Property(u => u.DeletionRequestedAt)
            .IsRequired(false);

        builder.Property(u => u.DeletionScheduledAt)
            .IsRequired(false);

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => new { u.IsActive, u.DeletedAt });

        // Relationships
        builder.HasMany(u => u.RelationshipsAsUser1)
            .WithOne(r => r.User1)
            .HasForeignKey(r => r.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.RelationshipsAsUser2)
            .WithOne(r => r.User2)
            .HasForeignKey(r => r.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(u => u.SentInvitations)
            .WithOne(i => i.Sender)
            .HasForeignKey(i => i.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ReceivedInvitations)
            .WithOne(i => i.Receiver)
            .HasForeignKey(i => i.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.UserStatus)
            .WithOne(s => s.User)
            .HasForeignKey<UserStatus>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

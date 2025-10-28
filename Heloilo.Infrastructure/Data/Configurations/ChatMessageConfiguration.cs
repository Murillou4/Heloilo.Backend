using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Content)
            .HasMaxLength(5000);

        builder.Property(cm => cm.MessageType)
            .HasConversion<string>()
            .HasDefaultValue(MessageType.Text);

        builder.Property(cm => cm.DeliveryStatus)
            .HasConversion<string>()
            .HasDefaultValue(DeliveryStatus.Sent);

        builder.Property(cm => cm.SentAt)
            .HasDefaultValueSql("datetime('now')");

        // Required properties moved from DataAnnotations
        builder.Property(cm => cm.MessageType)
            .IsRequired();

        builder.Property(cm => cm.DeliveryStatus)
            .IsRequired();

        // Indexes
        builder.HasIndex(cm => new { cm.RelationshipId, cm.SentAt });
        builder.HasIndex(cm => new { cm.SenderId, cm.SentAt });
        builder.HasIndex(cm => cm.DeliveryStatus);

        // Relationships
        builder.HasOne(cm => cm.Relationship)
            .WithMany(r => r.ChatMessages)
            .HasForeignKey(cm => cm.RelationshipId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cm => cm.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(cm => cm.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

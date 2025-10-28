using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Infrastructure.Data.Configurations;

public class RelationshipInvitationConfiguration : IEntityTypeConfiguration<RelationshipInvitation>
{
    public void Configure(EntityTypeBuilder<RelationshipInvitation> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasDefaultValue(InvitationStatus.Pending);

        builder.Property(i => i.SentAt)
            .HasDefaultValueSql("datetime('now')");

        // Constraints
        builder.ToTable(t => t.HasCheckConstraint("CK_Invitation_Users_Different", "SenderId != ReceiverId"));

        // Indexes
        builder.HasIndex(i => new { i.ReceiverId, i.Status });
        builder.HasIndex(i => new { i.SenderId, i.Status });

        // Relationships
        builder.HasOne(i => i.Sender)
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(i => i.SenderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Receiver)
            .WithMany(u => u.ReceivedInvitations)
            .HasForeignKey(i => i.ReceiverId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

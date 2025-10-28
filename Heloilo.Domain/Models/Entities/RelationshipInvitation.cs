using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class RelationshipInvitation : BaseEntity
{
    public long SenderId { get; set; }
    public long ReceiverId { get; set; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    public virtual User Sender { get; set; } = null!;
    public virtual User Receiver { get; set; } = null!;
}

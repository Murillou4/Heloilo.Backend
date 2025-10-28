using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class ChatMessage : BaseEntity, ISoftDeletable
{
    public long RelationshipId { get; set; }
    public long SenderId { get; set; }

    public string? Content { get; set; }

    public MessageType MessageType { get; set; } = MessageType.Text;

    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Sent;

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual Relationship Relationship { get; set; } = null!;
    public virtual User Sender { get; set; } = null!;
    public virtual ICollection<MessageMedia> Media { get; set; } = new List<MessageMedia>();
}

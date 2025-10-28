using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class Relationship : BaseEntity, ISoftDeletable
{
    public long User1Id { get; set; }
    public long User2Id { get; set; }

    public DateOnly? MetDate { get; set; }

    public string? MetLocation { get; set; }

    public DateOnly? RelationshipStartDate { get; set; }

    public CelebrationType CelebrationType { get; set; } = CelebrationType.Annual;

    public bool IsActive { get; set; } = true;

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual User User1 { get; set; } = null!;
    public virtual User User2 { get; set; } = null!;
    public virtual ICollection<InitialSetup> InitialSetups { get; set; } = new List<InitialSetup>();
    public virtual ICollection<Wish> Wishes { get; set; } = new List<Wish>();
    public virtual ICollection<Memory> Memories { get; set; } = new List<Memory>();
    public virtual ICollection<MoodLog> MoodLogs { get; set; } = new List<MoodLog>();
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<StoryPage> StoryPages { get; set; } = new List<StoryPage>();
}

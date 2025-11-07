using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class User : BaseEntity, ISoftDeletable
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }

    public byte[]? ProfilePhotoBlob { get; set; }

    public string ThemeColor { get; set; } = "#FF6B9D";

    public bool IsActive { get; set; } = true;

    public bool EmailVerified { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Relationship> RelationshipsAsUser1 { get; set; } = new List<Relationship>();
    public virtual ICollection<Relationship> RelationshipsAsUser2 { get; set; } = new List<Relationship>();
    public virtual ICollection<RelationshipInvitation> SentInvitations { get; set; } = new List<RelationshipInvitation>();
    public virtual ICollection<RelationshipInvitation> ReceivedInvitations { get; set; } = new List<RelationshipInvitation>();
    public virtual ICollection<InitialSetup> InitialSetups { get; set; } = new List<InitialSetup>();
    public virtual ICollection<Wish> Wishes { get; set; } = new List<Wish>();
    public virtual ICollection<WishComment> WishComments { get; set; } = new List<WishComment>();
    public virtual ICollection<MoodLog> MoodLogs { get; set; } = new List<MoodLog>();
    public virtual ICollection<DailyActivity> DailyActivities { get; set; } = new List<DailyActivity>();
    public virtual UserStatus? UserStatus { get; set; }
    public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public virtual ICollection<NotificationPreference> NotificationPreferences { get; set; } = new List<NotificationPreference>();
}

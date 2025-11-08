using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class Reminder : BaseEntity, ISoftDeletable
{
    public long UserId { get; set; }

    public long? RelationshipId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime ReminderDate { get; set; }

    public bool IsRecurring { get; set; } = false;

    public string? RecurrencePattern { get; set; } // "daily", "weekly", "monthly", "yearly"

    public bool IsCompleted { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Relationship? Relationship { get; set; }
}


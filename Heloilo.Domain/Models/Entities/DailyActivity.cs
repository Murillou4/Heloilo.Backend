using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class DailyActivity : BaseEntity, ISoftDeletable
{
    public long UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsCompleted { get; set; } = false;

    public int? ReminderMinutes { get; set; }

    public DateOnly ActivityDate { get; set; }

    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    public long? RecurrenceParentId { get; set; }

    public DateOnly? RecurrenceEndDate { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}

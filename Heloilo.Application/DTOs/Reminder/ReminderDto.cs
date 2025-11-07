namespace Heloilo.Application.DTOs.Reminder;

public class ReminderDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public long? RelationshipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ReminderDate { get; set; }
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateReminderDto
{
    public long? RelationshipId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ReminderDate { get; set; }
    public bool IsRecurring { get; set; } = false;
    public string? RecurrencePattern { get; set; }
}

public class UpdateReminderDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ReminderDate { get; set; }
    public bool? IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsActive { get; set; }
}


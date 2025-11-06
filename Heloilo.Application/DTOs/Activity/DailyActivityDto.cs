namespace Heloilo.Application.DTOs.Activity;

public class DailyActivityDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public int? ReminderMinutes { get; set; }
    public DateOnly ActivityDate { get; set; }
    public DateTime CreatedAt { get; set; }
}


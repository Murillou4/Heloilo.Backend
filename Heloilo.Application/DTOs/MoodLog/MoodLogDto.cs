namespace Heloilo.Application.DTOs.MoodLog;

public class MoodLogDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public long MoodTypeId { get; set; }
    public string MoodTypeName { get; set; } = string.Empty;
    public string MoodTypeEmoji { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateOnly LogDate { get; set; }
    public DateTime CreatedAt { get; set; }
}


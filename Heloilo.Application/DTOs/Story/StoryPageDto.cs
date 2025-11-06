namespace Heloilo.Application.DTOs.Story;

public class StoryPageDto
{
    public long Id { get; set; }
    public int PageNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public bool HasImage { get; set; }
    public DateOnly PageDate { get; set; }
    public DateTime CreatedAt { get; set; }
}


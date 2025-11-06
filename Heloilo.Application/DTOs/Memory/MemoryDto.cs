namespace Heloilo.Application.DTOs.Memory;

public class MemoryDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly MemoryDate { get; set; }
    public int MediaCount { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}


namespace Heloilo.Application.DTOs.Wish;

public class WishCategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Emoji { get; set; } = string.Empty;
    public string? Description { get; set; }
}


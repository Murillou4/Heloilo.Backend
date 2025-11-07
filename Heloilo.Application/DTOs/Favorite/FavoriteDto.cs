using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Favorite;

public class FavoriteDto
{
    public long Id { get; set; }
    public ContentType ContentType { get; set; }
    public long ContentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public object? Content { get; set; } // O conteúdo favoritado (Memory, Wish, StoryPage)
}


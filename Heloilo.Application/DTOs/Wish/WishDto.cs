using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Wish;

public class WishDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserNickname { get; set; }
    public long? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryEmoji { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LinkUrl { get; set; }
    public bool HasImage { get; set; }
    public int ImportanceLevel { get; set; }
    public WishStatus Status { get; set; }
    public DateTime? FulfilledAt { get; set; }
    public int CommentCount { get; set; }
    public DateTime CreatedAt { get; set; }
}


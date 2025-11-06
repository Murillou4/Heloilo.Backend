namespace Heloilo.Application.DTOs.Wish;

public class WishCommentDto
{
    public long Id { get; set; }
    public long WishId { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserNickname { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}


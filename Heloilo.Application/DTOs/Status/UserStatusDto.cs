namespace Heloilo.Application.DTOs.Status;

public class UserStatusDto
{
    public long Id { get; set; }
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTime StatusUpdatedAt { get; set; }
    public bool IsExpired { get; set; }
}


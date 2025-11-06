namespace Heloilo.Application.DTOs.User;

public class UserDto
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Nickname { get; set; }
    public string ThemeColor { get; set; } = string.Empty;
    public bool HasProfilePhoto { get; set; }
    public DateTime CreatedAt { get; set; }
}


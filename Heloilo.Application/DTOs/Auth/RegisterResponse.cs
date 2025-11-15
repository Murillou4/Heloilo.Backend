namespace Heloilo.Application.DTOs.Auth;
using Heloilo.Domain.Models.Entities;

public class RegisterResponse
{
    public User User { get; set; } = new User();
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool HasRelationship { get; set; } = false;
}


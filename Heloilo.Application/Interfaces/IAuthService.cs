using Heloilo.Application.DTOs.Auth;

namespace Heloilo.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    Task<bool> RevokeTokenAsync(string refreshToken);
    string? GetUserIdFromToken(string token);
}


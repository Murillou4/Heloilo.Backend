using Heloilo.Application.DTOs.User;
using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Interfaces;

public interface IUserService
{
    Task<UserDto> GetUserByIdAsync(long userId);
    Task<UserDto> GetCurrentUserAsync(long userId);
    Task<UserDto> UpdateUserAsync(long userId, UpdateUserDto dto);
    Task<UserDto> UpdateThemeAsync(long userId, UpdateThemeDto dto);
    Task<bool> UploadProfilePhotoAsync(long userId, IFormFile file);
    Task<byte[]?> GetProfilePhotoAsync(long userId);
    Task<bool> UpdatePasswordAsync(long userId, UpdatePasswordDto dto);
}


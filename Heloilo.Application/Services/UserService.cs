using Heloilo.Application.DTOs.User;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace Heloilo.Application.Services;

public class UserService : IUserService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(HeloiloDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserDto> GetUserByIdAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        return MapToDto(user);
    }

    public async Task<UserDto> GetCurrentUserAsync(long userId)
    {
        return await GetUserByIdAsync(userId);
    }

    public async Task<UserDto> UpdateUserAsync(long userId, UpdateUserDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        if (!string.IsNullOrEmpty(dto.Name))
        {
            user.Name = dto.Name;
        }

        if (dto.Nickname != null)
        {
            user.Nickname = dto.Nickname;
        }

        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateThemeAsync(long userId, UpdateThemeDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        user.ThemeColor = dto.ThemeColor;
        await _context.SaveChangesAsync();

        return MapToDto(user);
    }

    public async Task<bool> UploadProfilePhotoAsync(long userId, IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("Arquivo inválido");
        }

        // Validar arquivo
        var (isValid, errorMessage) = ValidationHelper.ValidateImageFile(file);
        if (!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        user.ProfilePhotoBlob = memoryStream.ToArray();

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<byte[]?> GetProfilePhotoAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        return user.ProfilePhotoBlob;
    }

    public async Task<bool> UpdatePasswordAsync(long userId, UpdatePasswordDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        // Verificar senha atual
        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Senha atual incorreta");
        }

        // Validar que a nova senha é diferente da atual
        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
        {
            throw new ArgumentException("A nova senha deve ser diferente da senha atual");
        }

        // Hash da nova senha
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _context.SaveChangesAsync();

        return true;
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Nickname = user.Nickname,
            ThemeColor = user.ThemeColor,
            HasProfilePhoto = user.ProfilePhotoBlob != null && user.ProfilePhotoBlob.Length > 0,
            CreatedAt = user.CreatedAt
        };
    }
}


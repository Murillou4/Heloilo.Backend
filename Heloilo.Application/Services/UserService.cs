using Heloilo.Application.DTOs.User;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
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

    public async Task<bool> RequestAccountDeletionAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        // Verificar se já existe uma solicitação pendente
        if (user.DeletionRequestedAt.HasValue && user.DeletionScheduledAt.HasValue && user.DeletionScheduledAt.Value > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Já existe uma solicitação de exclusão pendente");
        }

        // Definir período de graça de 30 dias
        user.DeletionRequestedAt = DateTime.UtcNow;
        user.DeletionScheduledAt = DateTime.UtcNow.AddDays(30);

        // Notificar parceiro se houver relacionamento ativo
        var relationship = await _context.Relationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship != null)
        {
            var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
            var partner = await _context.Users.FindAsync(partnerId);
            
            if (partner != null)
            {
                // Criar notificação para o parceiro
                var notification = new Notification
                {
                    UserId = partnerId,
                    RelationshipId = relationship.Id,
                    Title = "Solicitação de Exclusão de Conta",
                    Content = $"{user.Name} solicitou a exclusão da conta. A conta será excluída em {user.DeletionScheduledAt.Value:dd/MM/yyyy}.",
                    NotificationType = NotificationType.Anniversary,
                    IsRead = false,
                    SentAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelAccountDeletionAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        if (!user.DeletionRequestedAt.HasValue)
        {
            throw new InvalidOperationException("Não há solicitação de exclusão pendente");
        }

        // Cancelar exclusão
        user.DeletionRequestedAt = null;
        user.DeletionScheduledAt = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAccountAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        // Verificar se o período de graça já passou ou se é uma exclusão imediata
        if (user.DeletionScheduledAt.HasValue && user.DeletionScheduledAt.Value > DateTime.UtcNow)
        {
            throw new InvalidOperationException($"A exclusão só pode ser realizada após {user.DeletionScheduledAt.Value:dd/MM/yyyy}");
        }

        // Soft delete
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;
        user.DeletionRequestedAt = null;
        user.DeletionScheduledAt = null;

        // Desativar relacionamentos ativos
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .ToListAsync();

        foreach (var relationship in relationships)
        {
            relationship.IsActive = false;
            relationship.DeletedAt = DateTime.UtcNow;
        }

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


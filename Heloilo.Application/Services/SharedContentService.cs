using Heloilo.Application.DTOs.Shared;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Heloilo.Application.Services;

public class SharedContentService : ISharedContentService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<SharedContentService> _logger;
    private readonly IConfiguration _configuration;
    private const int DEFAULT_EXPIRATION_DAYS = 30;

    public SharedContentService(
        HeloiloDbContext context,
        ILogger<SharedContentService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SharedContentDto> CreateShareLinkAsync(long userId, ContentType contentType, long contentId, int? expirationDays = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        // Verificar se o conteúdo existe e pertence ao relacionamento
        if (!await ValidateContentExistsAsync(relationship.Id, contentType, contentId))
        {
            throw new KeyNotFoundException("Conteúdo não encontrado");
        }

        // Verificar se já existe um link ativo
        var existing = await _context.SharedContents
            .FirstOrDefaultAsync(s =>
                s.RelationshipId == relationship.Id &&
                s.ContentType == contentType &&
                s.ContentId == contentId &&
                !s.IsRevoked &&
                s.ExpiresAt > DateTime.UtcNow);

        if (existing != null)
        {
            // Retornar link existente
            return await MapToDtoAsync(existing);
        }

        // Criar novo link
        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddDays(expirationDays ?? DEFAULT_EXPIRATION_DAYS);

        var sharedContent = new SharedContent
        {
            RelationshipId = relationship.Id,
            ContentType = contentType,
            ContentId = contentId,
            Token = token,
            ExpiresAt = expiresAt
        };

        _context.SharedContents.Add(sharedContent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Link de compartilhamento criado: UserId={UserId}, ContentType={ContentType}, ContentId={ContentId}",
            userId, contentType, contentId);

        return await MapToDtoAsync(sharedContent);
    }

    public async Task<SharedContentDto?> GetSharedContentAsync(string token)
    {
        var sharedContent = await _context.SharedContents
            .FirstOrDefaultAsync(s =>
                s.Token == token &&
                !s.IsRevoked &&
                s.ExpiresAt > DateTime.UtcNow);

        if (sharedContent == null)
        {
            return null;
        }

        // Incrementar contador de acesso
        sharedContent.AccessCount++;
        sharedContent.LastAccessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await MapToDtoAsync(sharedContent, includeContent: true);
    }

    public async Task<bool> RevokeShareLinkAsync(long userId, string token)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        var sharedContent = await _context.SharedContents
            .FirstOrDefaultAsync(s =>
                s.Token == token &&
                s.RelationshipId == relationship.Id);

        if (sharedContent == null)
        {
            return false;
        }

        sharedContent.IsRevoked = true;
        sharedContent.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Link de compartilhamento revogado: UserId={UserId}, Token={Token}",
            userId, token);

        return true;
    }

    public async Task<bool> IncrementAccessCountAsync(string token)
    {
        var sharedContent = await _context.SharedContents
            .FirstOrDefaultAsync(s => s.Token == token);

        if (sharedContent == null)
        {
            return false;
        }

        sharedContent.AccessCount++;
        sharedContent.LastAccessedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    private async Task<SharedContentDto> MapToDtoAsync(SharedContent sharedContent, bool includeContent = false)
    {
        var baseUrl = _configuration["BaseUrl"] ?? "https://api.heloilo.com";
        var shareUrl = $"{baseUrl}/shared/{sharedContent.Token}";

        var dto = new SharedContentDto
        {
            Id = sharedContent.Id,
            ContentType = sharedContent.ContentType,
            ContentId = sharedContent.ContentId,
            Token = sharedContent.Token,
            ShareUrl = shareUrl,
            ExpiresAt = sharedContent.ExpiresAt,
            IsRevoked = sharedContent.IsRevoked,
            AccessCount = sharedContent.AccessCount
        };

        if (includeContent)
        {
            dto.Content = await LoadContentAsync(sharedContent.ContentType, sharedContent.ContentId, sharedContent.RelationshipId);
        }

        return dto;
    }

    private async Task<bool> ValidateContentExistsAsync(long relationshipId, ContentType contentType, long contentId)
    {
        return contentType switch
        {
            ContentType.Memory => await _context.Memories
                .AnyAsync(m => m.Id == contentId && m.RelationshipId == relationshipId && m.DeletedAt == null),
            ContentType.Wish => await _context.Wishes
                .AnyAsync(w => w.Id == contentId && w.RelationshipId == relationshipId && w.DeletedAt == null),
            ContentType.StoryPage => await _context.StoryPages
                .AnyAsync(s => s.Id == contentId && s.RelationshipId == relationshipId && s.DeletedAt == null),
            _ => false
        };
    }

    private async Task<object?> LoadContentAsync(ContentType contentType, long contentId, long relationshipId)
    {
        return contentType switch
        {
            ContentType.Memory => await _context.Memories
                .Include(m => m.Media)
                .Include(m => m.Tags)
                .FirstOrDefaultAsync(m => m.Id == contentId && m.RelationshipId == relationshipId && m.DeletedAt == null),
            ContentType.Wish => await _context.Wishes
                .Include(w => w.Category)
                .FirstOrDefaultAsync(w => w.Id == contentId && w.RelationshipId == relationshipId && w.DeletedAt == null),
            ContentType.StoryPage => await _context.StoryPages
                .FirstOrDefaultAsync(s => s.Id == contentId && s.RelationshipId == relationshipId && s.DeletedAt == null),
            _ => null
        };
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive &&
                r.DeletedAt == null);
    }

    private string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}


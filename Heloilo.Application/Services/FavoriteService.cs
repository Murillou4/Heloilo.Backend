using Heloilo.Application.DTOs.Favorite;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class FavoriteService : IFavoriteService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(HeloiloDbContext context, ILogger<FavoriteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> AddFavoriteAsync(long userId, ContentType contentType, long contentId)
    {
        // Verificar se já existe
        var existing = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ContentType == contentType && f.ContentId == contentId);

        if (existing != null)
        {
            return true; // Já é favorito
        }

        // Verificar se o conteúdo existe e pertence ao relacionamento do usuário
        if (!await ValidateContentExistsAsync(userId, contentType, contentId))
        {
            throw new KeyNotFoundException("Conteúdo não encontrado ou sem permissão");
        }

        var favorite = new Favorite
        {
            UserId = userId,
            ContentType = contentType,
            ContentId = contentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Favorito adicionado: UserId={UserId}, ContentType={ContentType}, ContentId={ContentId}",
            userId, contentType, contentId);

        return true;
    }

    public async Task<bool> RemoveFavoriteAsync(long userId, ContentType contentType, long contentId)
    {
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.ContentType == contentType && f.ContentId == contentId);

        if (favorite == null)
        {
            return false;
        }

        _context.Favorites.Remove(favorite);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Favorito removido: UserId={UserId}, ContentType={ContentType}, ContentId={ContentId}",
            userId, contentType, contentId);

        return true;
    }

    public async Task<bool> IsFavoriteAsync(long userId, ContentType contentType, long contentId)
    {
        return await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.ContentType == contentType && f.ContentId == contentId);
    }

    public async Task<PagedResult<FavoriteDto>> GetFavoritesAsync(long userId, ContentType? contentType = null, int page = 1, int pageSize = 20)
    {
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var query = _context.Favorites
            .Where(f => f.UserId == userId);

        if (contentType.HasValue)
        {
            query = query.Where(f => f.ContentType == contentType.Value);
        }

        var totalItems = await query.CountAsync();

        var favorites = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var favoriteDtos = favorites.Select(f => new FavoriteDto
        {
            Id = f.Id,
            ContentType = f.ContentType,
            ContentId = f.ContentId,
            CreatedAt = f.CreatedAt
        }).ToList();

        // Carregar conteúdo relacionado
        foreach (var dto in favoriteDtos)
        {
            dto.Content = await LoadContentAsync(dto.ContentType, dto.ContentId, userId);
        }

        return new PagedResult<FavoriteDto>
        {
            Items = favoriteDtos,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private async Task<bool> ValidateContentExistsAsync(long userId, ContentType contentType, long contentId)
    {
        // Obter relacionamento do usuário
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null)
        {
            return false;
        }

        return contentType switch
        {
            ContentType.Memory => await _context.Memories
                .AnyAsync(m => m.Id == contentId && m.RelationshipId == relationship.Id && m.DeletedAt == null),
            ContentType.Wish => await _context.Wishes
                .AnyAsync(w => w.Id == contentId && w.RelationshipId == relationship.Id && w.DeletedAt == null),
            ContentType.StoryPage => await _context.StoryPages
                .AnyAsync(s => s.Id == contentId && s.RelationshipId == relationship.Id && s.DeletedAt == null),
            _ => false
        };
    }

    private async Task<object?> LoadContentAsync(ContentType contentType, long contentId, long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null)
        {
            return null;
        }

        return contentType switch
        {
            ContentType.Memory => await _context.Memories
                .Include(m => m.Media)
                .Include(m => m.Tags)
                .FirstOrDefaultAsync(m => m.Id == contentId && m.RelationshipId == relationship.Id && m.DeletedAt == null),
            ContentType.Wish => await _context.Wishes
                .Include(w => w.Category)
                .FirstOrDefaultAsync(w => w.Id == contentId && w.RelationshipId == relationship.Id && w.DeletedAt == null),
            ContentType.StoryPage => await _context.StoryPages
                .FirstOrDefaultAsync(s => s.Id == contentId && s.RelationshipId == relationship.Id && s.DeletedAt == null),
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
}


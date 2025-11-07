using Heloilo.Application.DTOs.Wish;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class WishService : IWishService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<WishService> _logger;
    private readonly INotificationService _notificationService;

    public WishService(HeloiloDbContext context, ILogger<WishService> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<WishDto>> GetWishesAsync(long userId, long? categoryId = null, string? search = null, string? sortBy = null, string? sortOrder = null, WishStatus? status = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        var query = _context.Wishes
            .Include(w => w.User)
            .Include(w => w.Category)
            .Include(w => w.Comments)
            .Where(w => w.RelationshipId == relationship.Id && w.DeletedAt == null);

        if (categoryId.HasValue)
        {
            query = query.Where(w => w.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(w => w.Title.Contains(search));
        }

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        // Ordenação
        var isDescending = sortOrder?.ToLower() != "asc";
        query = sortBy?.ToLower() switch
        {
            "importance" => isDescending ? query.OrderByDescending(w => w.ImportanceLevel).ThenByDescending(w => w.CreatedAt) : query.OrderBy(w => w.ImportanceLevel).ThenBy(w => w.CreatedAt),
            "date" => isDescending ? query.OrderByDescending(w => w.CreatedAt) : query.OrderBy(w => w.CreatedAt),
            "title" => isDescending ? query.OrderByDescending(w => w.Title) : query.OrderBy(w => w.Title),
            _ => query.OrderByDescending(w => w.CreatedAt)
        };

        var totalItems = await query.CountAsync();

        var wishes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WishDto>
        {
            Items = wishes.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<WishDto> GetWishByIdAsync(long wishId, long userId)
    {
        var wish = await _context.Wishes
            .Include(w => w.User)
            .Include(w => w.Category)
            .Include(w => w.Comments)
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null)
        {
            throw new KeyNotFoundException("Desejo não encontrado");
        }

        // Verificar se o desejo pertence ao relacionamento do usuário
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || wish.RelationshipId != relationship.Id)
        {
            throw new UnauthorizedAccessException("Acesso negado a este desejo");
        }

        return MapToDto(wish);
    }

    public async Task<WishDto> CreateWishAsync(long userId, CreateWishDto dto, IFormFile? image = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        // Validar URL se fornecida
        if (!string.IsNullOrEmpty(dto.LinkUrl) && !ValidationHelper.IsValidUrl(dto.LinkUrl))
        {
            throw new ArgumentException("URL inválida. Use apenas HTTP ou HTTPS");
        }

        // Validar limites de caracteres
        if (!ValidationHelper.ValidateCharacterLimit(dto.Title, 500))
        {
            throw new ArgumentException("Título excede o limite de 500 caracteres");
        }

        if (!ValidationHelper.ValidateCharacterLimit(dto.Description, 2000))
        {
            throw new ArgumentException("Descrição excede o limite de 2000 caracteres");
        }

        byte[]? imageBlob = null;
        if (image != null)
        {
            var (isValid, errorMessage) = ValidationHelper.ValidateImageFile(image);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage);
            }

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            imageBlob = memoryStream.ToArray();
        }

        var wish = new Wish
        {
            UserId = userId,
            RelationshipId = relationship.Id,
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Description = dto.Description,
            LinkUrl = dto.LinkUrl,
            ImageBlob = imageBlob,
            ImportanceLevel = dto.ImportanceLevel
        };

        _context.Wishes.Add(wish);
        await _context.SaveChangesAsync();

        // Notificar o parceiro sobre o novo desejo
        try
        {
            var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
            await _notificationService.CreateAndSendNotificationAsync(
                partnerId,
                relationship.Id,
                "Novo desejo adicionado",
                $"Seu parceiro adicionou um novo desejo: {dto.Title}",
                NotificationType.Wish
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao criar notificação para novo desejo");
        }

        return await GetWishByIdAsync(wish.Id, userId);
    }

    public async Task<WishDto> UpdateWishAsync(long wishId, long userId, UpdateWishDto dto, IFormFile? image = null)
    {
        var wish = await _context.Wishes
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null)
        {
            throw new KeyNotFoundException("Desejo não encontrado");
        }

        if (wish.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você só pode editar seus próprios desejos");
        }

        // Validar URL se fornecida
        if (!string.IsNullOrEmpty(dto.LinkUrl) && !ValidationHelper.IsValidUrl(dto.LinkUrl))
        {
            throw new ArgumentException("URL inválida. Use apenas HTTP ou HTTPS");
        }

        if (!string.IsNullOrEmpty(dto.Title))
        {
            if (!ValidationHelper.ValidateCharacterLimit(dto.Title, 500))
            {
                throw new ArgumentException("Título excede o limite de 500 caracteres");
            }
            wish.Title = dto.Title;
        }

        if (dto.Description != null)
        {
            if (!ValidationHelper.ValidateCharacterLimit(dto.Description, 2000))
            {
                throw new ArgumentException("Descrição excede o limite de 2000 caracteres");
            }
            wish.Description = dto.Description;
        }

        if (dto.LinkUrl != null)
        {
            wish.LinkUrl = dto.LinkUrl;
        }

        if (dto.CategoryId.HasValue)
        {
            wish.CategoryId = dto.CategoryId.Value;
        }

        if (dto.ImportanceLevel.HasValue)
        {
            wish.ImportanceLevel = dto.ImportanceLevel.Value;
        }

        if (image != null)
        {
            var (isValid, errorMessage) = ValidationHelper.ValidateImageFile(image);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage);
            }

            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            wish.ImageBlob = memoryStream.ToArray();
        }

        await _context.SaveChangesAsync();

        return await GetWishByIdAsync(wishId, userId);
    }

    public async Task<bool> DeleteWishAsync(long wishId, long userId)
    {
        var wish = await _context.Wishes
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null)
        {
            throw new KeyNotFoundException("Desejo não encontrado");
        }

        if (wish.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você só pode excluir seus próprios desejos");
        }

        wish.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<WishCategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.WishCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return categories.Select(c => new WishCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Emoji = c.Emoji,
            Description = c.Description
        }).ToList();
    }

    public async Task<PagedResult<WishCommentDto>> GetWishCommentsAsync(long wishId, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var query = _context.WishComments
            .Include(c => c.User)
            .Where(c => c.WishId == wishId && c.DeletedAt == null)
            .OrderBy(c => c.CreatedAt);

        var totalItems = await query.CountAsync();

        var comments = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WishCommentDto>
        {
            Items = comments.Select(c => new WishCommentDto
            {
                Id = c.Id,
                WishId = c.WishId,
                UserId = c.UserId,
                UserName = c.User.Name,
                UserNickname = c.User.Nickname,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<WishCommentDto> AddCommentAsync(long wishId, long userId, CreateWishCommentDto dto)
    {
        var wish = await _context.Wishes
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null)
        {
            throw new KeyNotFoundException("Desejo não encontrado");
        }

        if (!ValidationHelper.ValidateCharacterLimit(dto.Content, 2000))
        {
            throw new ArgumentException("Comentário excede o limite de 2000 caracteres");
        }

        var comment = new WishComment
        {
            WishId = wishId,
            UserId = userId,
            Content = dto.Content
        };

        _context.WishComments.Add(comment);
        await _context.SaveChangesAsync();

        var commentWithUser = await _context.WishComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id);

        // Notificar o parceiro sobre o novo comentário
        try
        {
            var relationship = await GetRelationshipAsync(userId);
            if (relationship != null)
            {
                var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
                var wishTitle = wish.Title.Length > 50 ? wish.Title.Substring(0, 50) + "..." : wish.Title;
                await _notificationService.CreateAndSendNotificationAsync(
                    partnerId,
                    relationship.Id,
                    "Novo comentário em desejo",
                    $"Seu parceiro comentou no desejo: {wishTitle}",
                    NotificationType.Comment
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao criar notificação para novo comentário");
        }

        return new WishCommentDto
        {
            Id = comment.Id,
            WishId = comment.WishId,
            UserId = comment.UserId,
            UserName = commentWithUser!.User.Name,
            UserNickname = commentWithUser.User.Nickname,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<WishCommentDto> UpdateCommentAsync(long commentId, long userId, CreateWishCommentDto dto)
    {
        var comment = await _context.WishComments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comentário não encontrado");
        }

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você só pode editar seus próprios comentários");
        }

        if (!ValidationHelper.ValidateCharacterLimit(dto.Content, 2000))
        {
            throw new ArgumentException("Comentário excede o limite de 2000 caracteres");
        }

        comment.Content = dto.Content;
        comment.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new WishCommentDto
        {
            Id = comment.Id,
            WishId = comment.WishId,
            UserId = comment.UserId,
            UserName = comment.User.Name,
            UserNickname = comment.User.Nickname,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt
        };
    }

    public async Task<bool> DeleteCommentAsync(long commentId, long userId)
    {
        var comment = await _context.WishComments
            .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);

        if (comment == null)
        {
            throw new KeyNotFoundException("Comentário não encontrado");
        }

        if (comment.UserId != userId)
        {
            throw new UnauthorizedAccessException("Você só pode excluir seus próprios comentários");
        }

        comment.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<byte[]?> GetWishImageAsync(long wishId, long userId)
    {
        var wish = await _context.Wishes
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null) throw new KeyNotFoundException("Desejo não encontrado");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || wish.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        return wish.ImageBlob;
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);
    }

    private static WishDto MapToDto(Wish wish)
    {
        return new WishDto
        {
            Id = wish.Id,
            UserId = wish.UserId,
            UserName = wish.User.Name,
            UserNickname = wish.User.Nickname,
            CategoryId = wish.CategoryId,
            CategoryName = wish.Category?.Name,
            CategoryEmoji = wish.Category?.Emoji,
            Title = wish.Title,
            Description = wish.Description,
            LinkUrl = wish.LinkUrl,
            HasImage = wish.ImageBlob != null && wish.ImageBlob.Length > 0,
            ImportanceLevel = wish.ImportanceLevel,
            Status = wish.Status,
            FulfilledAt = wish.FulfilledAt,
            CommentCount = wish.Comments.Count(c => c.DeletedAt == null),
            CreatedAt = wish.CreatedAt
        };
    }

    public async Task<PagedResult<WishDto>> GetWishesByPriorityAsync(long userId, int? minImportanceLevel = null, int page = 1, int pageSize = 20)
    {
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.Wishes
            .Include(w => w.User)
            .Include(w => w.Category)
            .Include(w => w.Comments)
            .Where(w => w.RelationshipId == relationship.Id && w.DeletedAt == null && w.Status == WishStatus.Pending);

        if (minImportanceLevel.HasValue)
        {
            query = query.Where(w => w.ImportanceLevel >= minImportanceLevel.Value);
        }

        query = query.OrderByDescending(w => w.ImportanceLevel).ThenByDescending(w => w.CreatedAt);

        var totalItems = await query.CountAsync();

        var wishes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<WishDto>
        {
            Items = wishes.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<WishDto> FulfillWishAsync(long wishId, long userId)
    {
        var wish = await _context.Wishes
            .FirstOrDefaultAsync(w => w.Id == wishId && w.DeletedAt == null);

        if (wish == null) throw new KeyNotFoundException("Desejo não encontrado");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || wish.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        if (wish.Status == WishStatus.Fulfilled)
        {
            throw new InvalidOperationException("Desejo já foi realizado");
        }

        wish.Status = WishStatus.Fulfilled;
        wish.FulfilledAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Desejo marcado como realizado: WishId={WishId}, UserId={UserId}", wishId, userId);

        return await GetWishByIdAsync(wishId, userId);
    }
}


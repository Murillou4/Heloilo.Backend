using Heloilo.Application.DTOs.Story;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class StoryService : IStoryService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<StoryService> _logger;

    public StoryService(HeloiloDbContext context, ILogger<StoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<StoryPageDto>> GetStoryPagesAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.StoryPages
            .Where(p => p.RelationshipId == relationship.Id && p.DeletedAt == null);

        if (startDate.HasValue)
            query = query.Where(p => p.PageDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.PageDate <= endDate.Value);

        // Ordenação
        var isDescending = sortOrder?.ToLower() == "desc";
        query = sortBy?.ToLower() switch
        {
            "date" => isDescending ? query.OrderByDescending(p => p.PageDate) : query.OrderBy(p => p.PageDate),
            "number" => isDescending ? query.OrderByDescending(p => p.PageNumber) : query.OrderBy(p => p.PageNumber),
            "title" => isDescending ? query.OrderByDescending(p => p.Title) : query.OrderBy(p => p.Title),
            _ => query.OrderBy(p => p.PageNumber)
        };

        var totalItems = await query.CountAsync();

        var pages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<StoryPageDto>
        {
            Items = pages.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<StoryPageDto> GetStoryPageByIdAsync(long pageId, long userId)
    {
        var page = await _context.StoryPages
            .FirstOrDefaultAsync(p => p.Id == pageId && p.DeletedAt == null);

        if (page == null) throw new KeyNotFoundException("Página não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || page.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        return MapToDto(page);
    }

    public async Task<StoryPageDto> CreateStoryPageAsync(long userId, CreateStoryPageDto dto, IFormFile? image = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        // Obter próximo número de página
        var maxPageNumber = await _context.StoryPages
            .Where(p => p.RelationshipId == relationship.Id && p.DeletedAt == null)
            .Select(p => p.PageNumber)
            .DefaultIfEmpty(0)
            .MaxAsync();

        byte[]? imageBlob = null;
        if (image != null)
        {
            var (isValid, errorMessage) = ValidationHelper.ValidateImageFile(image);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage);
            }

            using var stream = new MemoryStream();
            await image.CopyToAsync(stream);
            imageBlob = stream.ToArray();
        }

        var page = new StoryPage
        {
            RelationshipId = relationship.Id,
            PageNumber = maxPageNumber + 1,
            Title = dto.Title,
            Content = dto.Content,
            PageDate = dto.PageDate,
            ImageBlob = imageBlob
        };

        _context.StoryPages.Add(page);
        await _context.SaveChangesAsync();

        return await GetStoryPageByIdAsync(page.Id, userId);
    }

    public async Task<StoryPageDto> UpdateStoryPageAsync(long pageId, long userId, CreateStoryPageDto dto, IFormFile? image = null)
    {
        var page = await _context.StoryPages
            .FirstOrDefaultAsync(p => p.Id == pageId && p.DeletedAt == null);

        if (page == null) throw new KeyNotFoundException("Página não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || page.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        page.Title = dto.Title;
        page.Content = dto.Content;
        page.PageDate = dto.PageDate;

        if (image != null)
        {
            var (isValid, errorMessage) = ValidationHelper.ValidateImageFile(image);
            if (!isValid)
            {
                throw new ArgumentException(errorMessage);
            }

            using var stream = new MemoryStream();
            await image.CopyToAsync(stream);
            page.ImageBlob = stream.ToArray();
        }

        await _context.SaveChangesAsync();

        return await GetStoryPageByIdAsync(pageId, userId);
    }

    public async Task<bool> DeleteStoryPageAsync(long pageId, long userId)
    {
        var page = await _context.StoryPages
            .FirstOrDefaultAsync(p => p.Id == pageId && p.DeletedAt == null);

        if (page == null) throw new KeyNotFoundException("Página não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || page.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        page.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<byte[]?> GetStoryPageImageAsync(long pageId, long userId)
    {
        var page = await _context.StoryPages
            .FirstOrDefaultAsync(p => p.Id == pageId && p.DeletedAt == null);

        if (page == null) throw new KeyNotFoundException("Página não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || page.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        return page.ImageBlob;
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }

    private static StoryPageDto MapToDto(StoryPage page)
    {
        return new StoryPageDto
        {
            Id = page.Id,
            PageNumber = page.PageNumber,
            Title = page.Title,
            Content = page.Content,
            HasImage = page.ImageBlob != null && page.ImageBlob.Length > 0,
            PageDate = page.PageDate,
            CreatedAt = page.CreatedAt
        };
    }
}


using Heloilo.Application.DTOs.Memory;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class MemoryService : IMemoryService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<MemoryService> _logger;

    public MemoryService(HeloiloDbContext context, ILogger<MemoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<MemoryDto>> GetMemoriesAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, List<string>? tags = null, string? search = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.Memories
            .Include(m => m.Media)
            .Include(m => m.Tags)
            .Where(m => m.RelationshipId == relationship.Id && m.DeletedAt == null);

        if (startDate.HasValue)
        {
            query = query.Where(m => m.MemoryDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(m => m.MemoryDate <= endDate.Value);
        }

        if (tags != null && tags.Any())
        {
            // Busca parcial e case-insensitive
            var normalizedTags = tags.Select(t => t.ToLowerInvariant().Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            if (normalizedTags.Any())
            {
                query = query.Where(m => m.Tags.Any(t => normalizedTags.Any(nt => t.TagName.ToLowerInvariant().Contains(nt))));
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Title.Contains(search) || (m.Description != null && m.Description.Contains(search)));
        }

        // Ordenação
        var isDescending = sortOrder?.ToLower() == "desc";
        query = sortBy?.ToLower() switch
        {
            "date" => isDescending ? query.OrderByDescending(m => m.MemoryDate) : query.OrderBy(m => m.MemoryDate),
            "title" => isDescending ? query.OrderByDescending(m => m.Title) : query.OrderBy(m => m.Title),
            "created" => isDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt),
            _ => query.OrderByDescending(m => m.MemoryDate)
        };

        var totalItems = await query.CountAsync();

        var memories = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MemoryDto>
        {
            Items = memories.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<MemoryDto> GetMemoryByIdAsync(long memoryId, long userId)
    {
        var memory = await _context.Memories
            .Include(m => m.Media)
            .Include(m => m.Tags)
            .FirstOrDefaultAsync(m => m.Id == memoryId && m.DeletedAt == null);

        if (memory == null) throw new KeyNotFoundException("Memória não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || memory.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        return MapToDto(memory);
    }

    public async Task<MemoryDto> CreateMemoryAsync(long userId, CreateMemoryDto dto, List<IFormFile>? media = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var memory = new Memory
        {
            RelationshipId = relationship.Id,
            Title = dto.Title,
            Description = dto.Description,
            MemoryDate = dto.MemoryDate
        };

        _context.Memories.Add(memory);
        await _context.SaveChangesAsync();

        // Adicionar tags
        if (dto.Tags != null && dto.Tags.Any())
        {
            foreach (var tagName in dto.Tags)
            {
                _context.MemoryTags.Add(new MemoryTag
                {
                    MemoryId = memory.Id,
                    TagName = tagName
                });
            }
            await _context.SaveChangesAsync();
        }

        // Adicionar mídia
        if (media != null && media.Any())
        {
            foreach (var file in media)
            {
                var (isValid, errorMessage) = ValidationHelper.ValidateMediaFile(file, allowVideo: true);
                if (!isValid)
                {
                    throw new ArgumentException(errorMessage);
                }

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                _context.MemoryMedia.Add(new MemoryMedia
                {
                    MemoryId = memory.Id,
                    FileBlob = stream.ToArray(),
                    FileType = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                    MimeType = file.ContentType
                });
            }
            await _context.SaveChangesAsync();
        }

        return await GetMemoryByIdAsync(memory.Id, userId);
    }

    public async Task<MemoryDto> UpdateMemoryAsync(long memoryId, long userId, CreateMemoryDto dto)
    {
        var memory = await _context.Memories
            .FirstOrDefaultAsync(m => m.Id == memoryId && m.DeletedAt == null);

        if (memory == null) throw new KeyNotFoundException("Memória não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || memory.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        memory.Title = dto.Title;
        memory.Description = dto.Description;
        memory.MemoryDate = dto.MemoryDate;

        await _context.SaveChangesAsync();

        return await GetMemoryByIdAsync(memoryId, userId);
    }

    public async Task<bool> DeleteMemoryAsync(long memoryId, long userId)
    {
        var memory = await _context.Memories
            .FirstOrDefaultAsync(m => m.Id == memoryId && m.DeletedAt == null);

        if (memory == null) throw new KeyNotFoundException("Memória não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || memory.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        memory.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<long> AddMediaAsync(long memoryId, long userId, IFormFile file)
    {
        var memory = await _context.Memories
            .FirstOrDefaultAsync(m => m.Id == memoryId && m.DeletedAt == null);

        if (memory == null) throw new KeyNotFoundException("Memória não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || memory.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        var (isValid, errorMessage) = ValidationHelper.ValidateMediaFile(file, allowVideo: true);
        if (!isValid)
        {
            throw new ArgumentException(errorMessage);
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        var media = new MemoryMedia
        {
            MemoryId = memoryId,
            FileBlob = stream.ToArray(),
            FileType = Path.GetExtension(file.FileName),
            FileSize = file.Length,
            MimeType = file.ContentType
        };

        _context.MemoryMedia.Add(media);
        await _context.SaveChangesAsync();

        return media.Id;
    }

    public async Task<byte[]?> GetMemoryMediaAsync(long mediaId, long userId)
    {
        var media = await _context.MemoryMedia
            .Include(m => m.Memory)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media == null) throw new KeyNotFoundException("Mídia não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || media.Memory.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        return media.FileBlob;
    }

    public async Task<bool> DeleteMediaAsync(long memoryId, long mediaId, long userId)
    {
        var memory = await _context.Memories
            .FirstOrDefaultAsync(m => m.Id == memoryId && m.DeletedAt == null);

        if (memory == null) throw new KeyNotFoundException("Memória não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || memory.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        var media = await _context.MemoryMedia
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.MemoryId == memoryId);

        if (media == null) throw new KeyNotFoundException("Mídia não encontrada");

        _context.MemoryMedia.Remove(media);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<string>> GetTagsAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var tags = await _context.MemoryTags
            .Where(t => t.Memory.RelationshipId == relationship.Id && t.Memory.DeletedAt == null)
            .Select(t => t.TagName)
            .Distinct()
            .ToListAsync();

        return tags;
    }

    public async Task<Dictionary<string, object>> GetMemoryStatsAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var memories = await _context.Memories
            .Include(m => m.Tags)
            .Include(m => m.Media)
            .Where(m => m.RelationshipId == relationship.Id && m.DeletedAt == null)
            .ToListAsync();

        var stats = new Dictionary<string, object>
        {
            { "totalMemories", memories.Count },
            { "totalMedia", memories.Sum(m => m.Media.Count) },
            { "memoriesWithMedia", memories.Count(m => m.Media.Any()) },
            { "memoriesWithTags", memories.Count(m => m.Tags.Any()) },
            { "totalTags", memories.SelectMany(m => m.Tags).Select(t => t.TagName).Distinct().Count() },
            { "byMonth", memories.GroupBy(m => new { m.MemoryDate.Year, m.MemoryDate.Month })
                .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                .Take(12)
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Count()) },
            { "byTag", memories.SelectMany(m => m.Tags)
                .GroupBy(t => t.TagName)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count()) },
            { "oldestMemory", memories.Any() ? memories.Min(m => m.MemoryDate).ToString("yyyy-MM-dd") : string.Empty },
            { "newestMemory", memories.Any() ? memories.Max(m => m.MemoryDate).ToString("yyyy-MM-dd") : string.Empty }
        };

        return stats;
    }

    public async Task<Dictionary<string, object>> GetMemoryTimelineAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.Memories
            .Include(m => m.Media)
            .Include(m => m.Tags)
            .Where(m => m.RelationshipId == relationship.Id && m.DeletedAt == null);

        if (startDate.HasValue)
        {
            query = query.Where(m => m.MemoryDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(m => m.MemoryDate <= endDate.Value);
        }

        var memories = await query
            .OrderBy(m => m.MemoryDate)
            .ToListAsync();

        // Agrupar por período (ano, mês, semana)
        var timeline = new Dictionary<string, object>
        {
            { "byYear", memories.GroupBy(m => m.MemoryDate.Year)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString(), g => g.Select(MapToDto).ToList()) },
            { "byMonth", memories.GroupBy(m => new { m.MemoryDate.Year, m.MemoryDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Select(MapToDto).ToList()) },
            { "totalPeriods", memories.GroupBy(m => new { m.MemoryDate.Year, m.MemoryDate.Month }).Count() }
        };

        return timeline;
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }

    private static MemoryDto MapToDto(Memory memory)
    {
        return new MemoryDto
        {
            Id = memory.Id,
            Title = memory.Title,
            Description = memory.Description,
            MemoryDate = memory.MemoryDate,
            MediaCount = memory.Media.Count,
            Tags = memory.Tags.Select(t => t.TagName).ToList(),
            CreatedAt = memory.CreatedAt
        };
    }
}


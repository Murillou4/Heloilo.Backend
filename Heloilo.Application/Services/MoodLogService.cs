using Heloilo.Application.DTOs.MoodLog;
using Heloilo.Application.Helpers;
using Heloilo.Application.Hubs;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class MoodLogService : IMoodLogService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<MoodLogService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public MoodLogService(HeloiloDbContext context, ILogger<MoodLogService> logger, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<MoodLogDto> CreateMoodLogAsync(long userId, CreateMoodLogDto dto)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var logDate = dto.LogDate ?? DateOnly.FromDateTime(DateTime.Today);

        var moodLog = new MoodLog
        {
            UserId = userId,
            RelationshipId = relationship.Id,
            MoodTypeId = dto.MoodTypeId,
            Comment = dto.Comment,
            LogDate = logDate
        };

        _context.MoodLogs.Add(moodLog);
        await _context.SaveChangesAsync();

        var moodLogDto = await GetMoodLogByIdAsync(moodLog.Id);
        await NotifyPartnerAsync(userId, "MoodLogCreated", moodLogDto);

        return moodLogDto;
    }

    public async Task<PagedResult<MoodLogDto>> GetMoodLogsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, long? moodTypeId = null, string? moodCategory = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.MoodLogs
            .Include(m => m.User)
            .Include(m => m.MoodType)
            .Where(m => m.RelationshipId == relationship.Id && m.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(m => m.LogDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.LogDate <= endDate.Value);

        if (moodTypeId.HasValue)
            query = query.Where(m => m.MoodTypeId == moodTypeId.Value);

        if (!string.IsNullOrWhiteSpace(moodCategory) && Enum.TryParse<MoodCategory>(moodCategory, true, out var category))
            query = query.Where(m => m.MoodType.MoodCategory == category);

        // Ordenação
        var isDescending = sortOrder?.ToLower() == "desc";
        query = sortBy?.ToLower() switch
        {
            "date" => isDescending ? query.OrderByDescending(m => m.LogDate) : query.OrderBy(m => m.LogDate),
            "created" => isDescending ? query.OrderByDescending(m => m.CreatedAt) : query.OrderBy(m => m.CreatedAt),
            _ => query.OrderByDescending(m => m.LogDate).ThenByDescending(m => m.CreatedAt)
        };

        var totalItems = await query.CountAsync();

        var logs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MoodLogDto>
        {
            Items = logs.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<MoodDashboardDto> GetDashboardAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.MoodLogs
            .Include(m => m.MoodType)
            .Where(m => m.RelationshipId == relationship.Id && m.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(m => m.LogDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.LogDate <= endDate.Value);

        var logs = await query.ToListAsync();
        var distribution = logs.GroupBy(m => m.MoodType.Name)
            .ToDictionary(g => g.Key, g => g.Count());

        var recentLogs = logs
            .OrderByDescending(m => m.LogDate)
            .ThenByDescending(m => m.CreatedAt)
            .Take(10)
            .Select(MapToDto)
            .ToList();

        // Calcular score médio: positivo=+1, negativo=-1, neutro=0
        double averageMoodScore = 0;
        if (logs.Any())
        {
            var totalScore = 0.0;
            foreach (var log in logs)
            {
                var score = log.MoodType.MoodCategory switch
                {
                    MoodCategory.Positive => 1.0,
                    MoodCategory.Negative => -1.0,
                    MoodCategory.Neutral => 0.0,
                    _ => 0.0
                };
                totalScore += score;
            }
            averageMoodScore = totalScore / logs.Count;
        }

        return new MoodDashboardDto
        {
            MoodDistribution = distribution,
            RecentLogs = recentLogs,
            AverageMoodScore = averageMoodScore
        };
    }

    public async Task<PagedResult<MoodLogDto>> GetPartnerMoodLogsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;

        var query = _context.MoodLogs
            .Include(m => m.User)
            .Include(m => m.MoodType)
            .Where(m => m.RelationshipId == relationship.Id && m.UserId == partnerId);

        if (startDate.HasValue)
            query = query.Where(m => m.LogDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(m => m.LogDate <= endDate.Value);

        var totalItems = await query.CountAsync();

        var logs = await query
            .OrderByDescending(m => m.LogDate)
            .ThenByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<MoodLogDto>
        {
            Items = logs.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<List<MoodLogDto>> GetTodayTimelineAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var today = DateOnly.FromDateTime(DateTime.Today);

        var logs = await _context.MoodLogs
            .Include(m => m.User)
            .Include(m => m.MoodType)
            .Where(m => m.RelationshipId == relationship.Id && m.UserId == userId && m.LogDate == today)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        return logs.Select(MapToDto).ToList();
    }

    public async Task<List<object>> GetMoodTypesAsync()
    {
        var types = await _context.MoodTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.MoodCategory)
            .ThenBy(t => t.Name)
            .ToListAsync();

        return types.Select(t => new
        {
            t.Id,
            t.Name,
            t.Emoji,
            t.MoodCategory,
            t.Description
        }).Cast<object>().ToList();
    }

    private async Task<MoodLogDto> GetMoodLogByIdAsync(long id)
    {
        var log = await _context.MoodLogs
            .Include(m => m.User)
            .Include(m => m.MoodType)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (log == null) throw new KeyNotFoundException("Registro de humor não encontrado");

        return MapToDto(log);
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }

    private async Task NotifyPartnerAsync(long userId, string eventName, object payload)
    {
        try
        {
            var relationship = await GetRelationshipAsync(userId);
            if (relationship == null) return;

            var partnerId = RelationshipValidationHelper.GetPartnerId(relationship, userId);
            await _hubContext.Clients.Group($"user:{partnerId}")
                .SendAsync(eventName, payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar evento {EventName} para o parceiro do usuário {UserId}", eventName, userId);
        }
    }

    private static MoodLogDto MapToDto(MoodLog log)
    {
        return new MoodLogDto
        {
            Id = log.Id,
            UserId = log.UserId,
            UserName = log.User.Name,
            MoodTypeId = log.MoodTypeId,
            MoodTypeName = log.MoodType.Name,
            MoodTypeEmoji = log.MoodType.Emoji,
            Comment = log.Comment,
            LogDate = log.LogDate,
            CreatedAt = log.CreatedAt
        };
    }
}


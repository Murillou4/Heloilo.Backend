using Heloilo.Application.DTOs.Activity;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class ActivityService : IActivityService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(HeloiloDbContext context, ILogger<ActivityService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PagedResult<DailyActivityDto>> GetActivitiesAsync(long userId, DateOnly? date = null, DateOnly? startDate = null, DateOnly? endDate = null, bool? isCompleted = null, bool? hasReminder = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var query = _context.DailyActivities
            .Include(a => a.User)
            .Where(a => a.UserId == userId && a.DeletedAt == null);

        if (date.HasValue)
            query = query.Where(a => a.ActivityDate == date.Value);
        else
        {
            if (startDate.HasValue)
                query = query.Where(a => a.ActivityDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(a => a.ActivityDate <= endDate.Value);
        }

        if (isCompleted.HasValue)
            query = query.Where(a => a.IsCompleted == isCompleted.Value);

        if (hasReminder.HasValue)
            query = query.Where(a => hasReminder.Value ? a.ReminderMinutes != null : a.ReminderMinutes == null);

        // Ordenação
        var isDescending = sortOrder?.ToLower() == "desc";
        query = sortBy?.ToLower() switch
        {
            "date" => isDescending ? query.OrderByDescending(a => a.ActivityDate) : query.OrderBy(a => a.ActivityDate),
            "title" => isDescending ? query.OrderByDescending(a => a.Title) : query.OrderBy(a => a.Title),
            "created" => isDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => query.OrderBy(a => a.ActivityDate).ThenBy(a => a.CreatedAt)
        };

        var totalItems = await query.CountAsync();

        var activities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<DailyActivityDto>
        {
            Items = activities.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<DailyActivityDto> GetActivityByIdAsync(long activityId, long userId)
    {
        var activity = await _context.DailyActivities
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == activityId && a.DeletedAt == null);

        if (activity == null) throw new KeyNotFoundException("Atividade não encontrada");

        if (activity.UserId != userId)
            throw new UnauthorizedAccessException("Acesso negado");

        return MapToDto(activity);
    }

    public async Task<DailyActivityDto> CreateActivityAsync(long userId, CreateActivityDto dto)
    {
        var activity = new DailyActivity
        {
            UserId = userId,
            Title = dto.Title,
            Description = dto.Description,
            ActivityDate = dto.ActivityDate,
            ReminderMinutes = dto.ReminderMinutes,
            IsCompleted = false
        };

        _context.DailyActivities.Add(activity);
        await _context.SaveChangesAsync();

        return await GetActivityByIdAsync(activity.Id, userId);
    }

    public async Task<DailyActivityDto> UpdateActivityAsync(long activityId, long userId, CreateActivityDto dto)
    {
        var activity = await _context.DailyActivities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.DeletedAt == null);

        if (activity == null) throw new KeyNotFoundException("Atividade não encontrada");

        if (activity.UserId != userId)
            throw new UnauthorizedAccessException("Acesso negado");

        activity.Title = dto.Title;
        activity.Description = dto.Description;
        activity.ActivityDate = dto.ActivityDate;
        activity.ReminderMinutes = dto.ReminderMinutes;

        await _context.SaveChangesAsync();

        return await GetActivityByIdAsync(activityId, userId);
    }

    public async Task<bool> DeleteActivityAsync(long activityId, long userId)
    {
        var activity = await _context.DailyActivities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.DeletedAt == null);

        if (activity == null) throw new KeyNotFoundException("Atividade não encontrada");

        if (activity.UserId != userId)
            throw new UnauthorizedAccessException("Acesso negado");

        activity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<DailyActivityDto> MarkAsCompletedAsync(long activityId, long userId)
    {
        var activity = await _context.DailyActivities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.DeletedAt == null);

        if (activity == null) throw new KeyNotFoundException("Atividade não encontrada");

        if (activity.UserId != userId)
            throw new UnauthorizedAccessException("Acesso negado");

        activity.IsCompleted = true;
        await _context.SaveChangesAsync();

        return await GetActivityByIdAsync(activityId, userId);
    }

    public async Task<PagedResult<DailyActivityDto>> GetPartnerActivitiesAsync(long userId, DateOnly? date = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;

        var query = _context.DailyActivities
            .Include(a => a.User)
            .Where(a => a.UserId == partnerId && a.DeletedAt == null);

        if (date.HasValue)
            query = query.Where(a => a.ActivityDate == date.Value);

        var totalItems = await query.CountAsync();

        var activities = await query
            .OrderBy(a => a.ActivityDate)
            .ThenBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<DailyActivityDto>
        {
            Items = activities.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }

    private static DailyActivityDto MapToDto(DailyActivity activity)
    {
        return new DailyActivityDto
        {
            Id = activity.Id,
            UserId = activity.UserId,
            UserName = activity.User.Name,
            Title = activity.Title,
            Description = activity.Description,
            IsCompleted = activity.IsCompleted,
            ReminderMinutes = activity.ReminderMinutes,
            ActivityDate = activity.ActivityDate,
            CreatedAt = activity.CreatedAt
        };
    }
}


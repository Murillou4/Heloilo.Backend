using Heloilo.Application.DTOs.Status;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class UserStatusService : IUserStatusService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<UserStatusService> _logger;

    public UserStatusService(HeloiloDbContext context, ILogger<UserStatusService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserStatusDto> GetCurrentStatusAsync(long userId)
    {
        var status = await _context.UserStatuses
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (status == null)
        {
            return new UserStatusDto
            {
                Id = 0,
                UserId = userId,
                UserName = (await _context.Users.FindAsync(userId))?.Name ?? "",
                CurrentStatus = "",
                StatusUpdatedAt = DateTime.UtcNow,
                IsExpired = true
            };
        }

        var isExpired = (DateTime.UtcNow - status.StatusUpdatedAt).TotalHours > 4;

        return new UserStatusDto
        {
            Id = status.Id,
            UserId = status.UserId,
            UserName = status.User.Name,
            CurrentStatus = status.CurrentStatus,
            StatusUpdatedAt = status.StatusUpdatedAt,
            IsExpired = isExpired
        };
    }

    public async Task<UserStatusDto> UpdateStatusAsync(long userId, UpdateStatusDto dto)
    {
        var status = await _context.UserStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (status == null)
        {
            status = new UserStatus
            {
                UserId = userId,
                CurrentStatus = dto.CurrentStatus,
                StatusUpdatedAt = DateTime.UtcNow
            };
            _context.UserStatuses.Add(status);
        }
        else
        {
            status.CurrentStatus = dto.CurrentStatus;
            status.StatusUpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await GetCurrentStatusAsync(userId);
    }

    public async Task<UserStatusDto?> GetPartnerStatusAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) return null;

        var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;

        var status = await _context.UserStatuses
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == partnerId);

        if (status == null) return null;

        var isExpired = (DateTime.UtcNow - status.StatusUpdatedAt).TotalHours > 4;

        return new UserStatusDto
        {
            Id = status.Id,
            UserId = status.UserId,
            UserName = status.User.Name,
            CurrentStatus = status.CurrentStatus,
            StatusUpdatedAt = status.StatusUpdatedAt,
            IsExpired = isExpired
        };
    }

    public async Task<PagedResult<UserStatusDto>> GetStatusHistoryAsync(long userId, DateOnly? date = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var query = _context.UserStatuses
            .Include(s => s.User)
            .Where(s => s.UserId == userId);

        if (date.HasValue)
        {
            var startDate = date.Value.ToDateTime(TimeOnly.MinValue);
            var endDate = date.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(s => s.StatusUpdatedAt >= startDate && s.StatusUpdatedAt <= endDate);
        }

        var totalItems = await query.CountAsync();

        var statuses = await query
            .OrderByDescending(s => s.StatusUpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserStatusDto>
        {
            Items = statuses.Select(s => new UserStatusDto
        {
            Id = s.Id,
            UserId = s.UserId,
            UserName = s.User.Name,
            CurrentStatus = s.CurrentStatus,
            StatusUpdatedAt = s.StatusUpdatedAt,
            IsExpired = (DateTime.UtcNow - s.StatusUpdatedAt).TotalHours > 4
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<PagedResult<UserStatusDto>> GetPartnerStatusHistoryAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;

        var query = _context.UserStatuses
            .Include(s => s.User)
            .Where(s => s.UserId == partnerId);

        if (startDate.HasValue)
        {
            var start = startDate.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(s => s.StatusUpdatedAt >= start);
        }

        if (endDate.HasValue)
        {
            var end = endDate.Value.ToDateTime(TimeOnly.MaxValue);
            query = query.Where(s => s.StatusUpdatedAt <= end);
        }

        var totalItems = await query.CountAsync();

        var statuses = await query
            .OrderByDescending(s => s.StatusUpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<UserStatusDto>
        {
            Items = statuses.Select(s => new UserStatusDto
            {
                Id = s.Id,
                UserId = s.UserId,
                UserName = s.User.Name,
                CurrentStatus = s.CurrentStatus,
                StatusUpdatedAt = s.StatusUpdatedAt,
                IsExpired = (DateTime.UtcNow - s.StatusUpdatedAt).TotalHours > 4
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<bool> IsStatusExpiredAsync(long userId)
    {
        var status = await _context.UserStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (status == null) return true;

        return (DateTime.UtcNow - status.StatusUpdatedAt).TotalHours > 4;
    }

    public async Task<UserStatusDto> ClearExpiredStatusAsync(long userId)
    {
        var status = await _context.UserStatuses
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (status != null && (DateTime.UtcNow - status.StatusUpdatedAt).TotalHours > 4)
        {
            status.CurrentStatus = "Status expirado";
            status.StatusUpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return await GetCurrentStatusAsync(userId);
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }
}


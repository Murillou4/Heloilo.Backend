using Heloilo.Application.DTOs.Reminder;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class ReminderService : IReminderService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<ReminderService> _logger;
    private readonly INotificationService _notificationService;

    public ReminderService(HeloiloDbContext context, ILogger<ReminderService> logger, INotificationService notificationService)
    {
        _context = context;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<PagedResult<ReminderDto>> GetRemindersAsync(long userId, DateTime? startDate = null, DateTime? endDate = null, bool? isCompleted = null, bool? isActive = null, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = _context.Reminders
            .Where(r => r.UserId == userId && r.DeletedAt == null);

        if (startDate.HasValue)
            query = query.Where(r => r.ReminderDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(r => r.ReminderDate <= endDate.Value);
        if (isCompleted.HasValue)
            query = query.Where(r => r.IsCompleted == isCompleted.Value);
        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        var totalItems = await query.CountAsync();

        var reminders = await query
            .OrderBy(r => r.ReminderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ReminderDto>
        {
            Items = reminders.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<ReminderDto> GetReminderByIdAsync(long reminderId, long userId)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId && r.DeletedAt == null);

        if (reminder == null)
            throw new KeyNotFoundException("Lembrete não encontrado");

        return MapToDto(reminder);
    }

    public async Task<ReminderDto> CreateReminderAsync(long userId, CreateReminderDto dto)
    {
        // Validar relacionamento se fornecido
        if (dto.RelationshipId.HasValue)
        {
            var relationship = await _context.Relationships
                .FirstOrDefaultAsync(r => r.Id == dto.RelationshipId.Value &&
                                         (r.User1Id == userId || r.User2Id == userId) &&
                                         r.IsActive && r.DeletedAt == null);

            if (relationship == null)
                throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        var reminder = new Reminder
        {
            UserId = userId,
            RelationshipId = dto.RelationshipId,
            Title = dto.Title,
            Description = dto.Description,
            ReminderDate = dto.ReminderDate,
            IsRecurring = dto.IsRecurring,
            RecurrencePattern = dto.RecurrencePattern,
            IsActive = true,
            IsCompleted = false
        };

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        return MapToDto(reminder);
    }

    public async Task<ReminderDto> UpdateReminderAsync(long reminderId, long userId, UpdateReminderDto dto)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId && r.DeletedAt == null);

        if (reminder == null)
            throw new KeyNotFoundException("Lembrete não encontrado");

        if (!string.IsNullOrEmpty(dto.Title))
            reminder.Title = dto.Title;
        if (dto.Description != null)
            reminder.Description = dto.Description;
        if (dto.ReminderDate.HasValue)
            reminder.ReminderDate = dto.ReminderDate.Value;
        if (dto.IsRecurring.HasValue)
            reminder.IsRecurring = dto.IsRecurring.Value;
        if (dto.RecurrencePattern != null)
            reminder.RecurrencePattern = dto.RecurrencePattern;
        if (dto.IsCompleted.HasValue)
        {
            reminder.IsCompleted = dto.IsCompleted.Value;
            if (dto.IsCompleted.Value && !reminder.CompletedAt.HasValue)
                reminder.CompletedAt = DateTime.UtcNow;
            else if (!dto.IsCompleted.Value)
                reminder.CompletedAt = null;
        }
        if (dto.IsActive.HasValue)
            reminder.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return MapToDto(reminder);
    }

    public async Task<bool> DeleteReminderAsync(long reminderId, long userId)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId && r.DeletedAt == null);

        if (reminder == null)
            throw new KeyNotFoundException("Lembrete não encontrado");

        reminder.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ReminderDto> MarkAsCompletedAsync(long reminderId, long userId)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && r.UserId == userId && r.DeletedAt == null);

        if (reminder == null)
            throw new KeyNotFoundException("Lembrete não encontrado");

        reminder.IsCompleted = true;
        reminder.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(reminder);
    }

    private static ReminderDto MapToDto(Reminder reminder)
    {
        return new ReminderDto
        {
            Id = reminder.Id,
            UserId = reminder.UserId,
            RelationshipId = reminder.RelationshipId,
            Title = reminder.Title,
            Description = reminder.Description,
            ReminderDate = reminder.ReminderDate,
            IsRecurring = reminder.IsRecurring,
            RecurrencePattern = reminder.RecurrencePattern,
            IsCompleted = reminder.IsCompleted,
            CompletedAt = reminder.CompletedAt,
            IsActive = reminder.IsActive,
            CreatedAt = reminder.CreatedAt,
            UpdatedAt = reminder.UpdatedAt
        };
    }
}


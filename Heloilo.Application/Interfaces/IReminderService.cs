using Heloilo.Application.DTOs.Reminder;
using Heloilo.Domain.Models.Common;

namespace Heloilo.Application.Interfaces;

public interface IReminderService
{
    Task<PagedResult<ReminderDto>> GetRemindersAsync(long userId, DateTime? startDate = null, DateTime? endDate = null, bool? isCompleted = null, bool? isActive = null, int page = 1, int pageSize = 20);
    Task<ReminderDto> GetReminderByIdAsync(long reminderId, long userId);
    Task<ReminderDto> CreateReminderAsync(long userId, CreateReminderDto dto);
    Task<ReminderDto> UpdateReminderAsync(long reminderId, long userId, UpdateReminderDto dto);
    Task<bool> DeleteReminderAsync(long reminderId, long userId);
    Task<ReminderDto> MarkAsCompletedAsync(long reminderId, long userId);
}


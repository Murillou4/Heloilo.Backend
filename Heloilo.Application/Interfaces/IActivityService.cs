using Heloilo.Application.DTOs.Activity;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.Interfaces;

public interface IActivityService
{
    Task<PagedResult<DailyActivityDto>> GetActivitiesAsync(long userId, DateOnly? date = null, DateOnly? startDate = null, DateOnly? endDate = null, bool? isCompleted = null, bool? hasReminder = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20);
    Task<DailyActivityDto> GetActivityByIdAsync(long activityId, long userId);
    Task<DailyActivityDto> CreateActivityAsync(long userId, CreateActivityDto dto);
    Task<DailyActivityDto> UpdateActivityAsync(long activityId, long userId, CreateActivityDto dto);
    Task<bool> DeleteActivityAsync(long activityId, long userId);
    Task<DailyActivityDto> MarkAsCompletedAsync(long activityId, long userId);
    Task<PagedResult<DailyActivityDto>> GetPartnerActivitiesAsync(long userId, DateOnly? date = null, int page = 1, int pageSize = 20);
    Task<PagedResult<DailyActivityDto>> GetRecurringActivitiesAsync(long userId, int page = 1, int pageSize = 20);
    Task<DailyActivityDto> CreateRecurrenceAsync(long activityId, long userId, RecurrenceType recurrenceType, DateOnly? endDate = null);
    Task<Dictionary<string, object>> GetActivitiesCalendarAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null);
}


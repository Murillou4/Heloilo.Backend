using Heloilo.Application.DTOs.MoodLog;
using Heloilo.Domain.Models.Common;

namespace Heloilo.Application.Interfaces;

public interface IMoodLogService
{
    Task<MoodLogDto> CreateMoodLogAsync(long userId, CreateMoodLogDto dto);
    Task<PagedResult<MoodLogDto>> GetMoodLogsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, long? moodTypeId = null, string? moodCategory = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20);
    Task<MoodDashboardDto> GetDashboardAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null);
    Task<PagedResult<MoodLogDto>> GetPartnerMoodLogsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, int page = 1, int pageSize = 20);
    Task<List<MoodLogDto>> GetTodayTimelineAsync(long userId);
    Task<List<object>> GetMoodTypesAsync();
}


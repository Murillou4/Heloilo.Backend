namespace Heloilo.Application.Interfaces;

public interface IAnalyticsService
{
    Task<Dictionary<string, object>> GetMoodTrendsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null);
    Task<Dictionary<string, object>> GetActivityPatternsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null);
    Task<Dictionary<string, object>> GetCommunicationStatsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null);
    Task<Dictionary<string, object>> GetAnnualReportAsync(long userId, int year);
    Task<Dictionary<string, object>> GetMonthlyReportAsync(long userId, int year, int month);
}


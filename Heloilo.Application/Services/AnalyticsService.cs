using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(HeloiloDbContext context, ILogger<AnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Dictionary<string, object>> GetMoodTrendsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.MoodLogs
            .Include(m => m.MoodType)
            .Where(m => m.RelationshipId == relationship.Id);

        if (startDate.HasValue)
            query = query.Where(m => m.LogDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(m => m.LogDate <= endDate.Value);

        var moodLogs = await query.OrderBy(m => m.LogDate).ToListAsync();

        var trends = new Dictionary<string, object>
        {
            { "totalLogs", moodLogs.Count },
            { "byCategory", moodLogs.GroupBy(m => m.MoodType.MoodCategory)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()) },
            { "byMoodType", moodLogs.GroupBy(m => m.MoodType.Name)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .ToDictionary(g => g.Key, g => g.Count()) },
            { "byMonth", moodLogs.GroupBy(m => new { m.LogDate.Year, m.LogDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => new
                {
                    total = g.Count(),
                    positive = g.Count(m => m.MoodType.MoodCategory == MoodCategory.Positive),
                    negative = g.Count(m => m.MoodType.MoodCategory == MoodCategory.Negative),
                    neutral = g.Count(m => m.MoodType.MoodCategory == MoodCategory.Neutral)
                }) },
            { "averagePerDay", moodLogs.Any() ? (double)moodLogs.Count / Math.Max(1, (endDate ?? DateOnly.FromDateTime(DateTime.Today)).DayNumber - (startDate ?? moodLogs.Min(m => m.LogDate)).DayNumber) : 0 },
            { "mostCommonMood", moodLogs.GroupBy(m => m.MoodType.Name)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "N/A" }
        };

        return trends;
    }

    public async Task<Dictionary<string, object>> GetActivityPatternsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = _context.DailyActivities
            .Where(a => a.UserId == userId && a.DeletedAt == null);

        if (startDate.HasValue)
            query = query.Where(a => a.ActivityDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(a => a.ActivityDate <= endDate.Value);

        var activities = await query.ToListAsync();

        var patterns = new Dictionary<string, object>
        {
            { "totalActivities", activities.Count },
            { "completedActivities", activities.Count(a => a.IsCompleted) },
            { "completionRate", activities.Any() ? (double)activities.Count(a => a.IsCompleted) / activities.Count * 100 : 0 },
            { "byMonth", activities.GroupBy(a => new { a.ActivityDate.Year, a.ActivityDate.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => new
                {
                    total = g.Count(),
                    completed = g.Count(a => a.IsCompleted),
                    completionRate = g.Any() ? (double)g.Count(a => a.IsCompleted) / g.Count() * 100 : 0
                }) },
            { "byDayOfWeek", activities.GroupBy(a => a.ActivityDate.DayOfWeek)
                .ToDictionary(g => g.Key.ToString(), g => new
                {
                    total = g.Count(),
                    completed = g.Count(a => a.IsCompleted)
                }) },
            { "averagePerDay", activities.Any() ? (double)activities.Count / Math.Max(1, (endDate ?? DateOnly.FromDateTime(DateTime.Today)).DayNumber - (startDate ?? activities.Min(a => a.ActivityDate)).DayNumber) : 0 }
        };

        return patterns;
    }

    public async Task<Dictionary<string, object>> GetCommunicationStatsAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.ChatMessages
            .Where(m => m.RelationshipId == relationship.Id && m.DeletedAt == null);

        if (startDate.HasValue)
            query = query.Where(m => m.SentAt.Date >= startDate.Value.ToDateTime(TimeOnly.MinValue));
        if (endDate.HasValue)
            query = query.Where(m => m.SentAt.Date <= endDate.Value.ToDateTime(TimeOnly.MinValue));

        var messages = await query.ToListAsync();

        var stats = new Dictionary<string, object>
        {
            { "totalMessages", messages.Count },
            { "sentByUser", messages.Count(m => m.SenderId == userId) },
            { "sentByPartner", messages.Count(m => m.SenderId != userId) },
            { "byType", messages.GroupBy(m => m.MessageType)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()) },
            { "byMonth", messages.GroupBy(m => new { m.SentAt.Year, m.SentAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .ToDictionary(g => $"{g.Key.Year}-{g.Key.Month:D2}", g => g.Count()) },
            { "byDayOfWeek", messages.GroupBy(m => m.SentAt.DayOfWeek)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()) },
            { "byHour", messages.GroupBy(m => m.SentAt.Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()) },
            { "averagePerDay", messages.Any() ? (double)messages.Count / Math.Max(1, (endDate ?? DateOnly.FromDateTime(DateTime.Today)).DayNumber - (startDate ?? DateOnly.FromDateTime(messages.Min(m => m.SentAt).Date)).DayNumber) : 0 },
            { "readRate", messages.Any() ? (double)messages.Count(m => m.ReadAt != null) / messages.Count * 100 : 0 }
        };

        return stats;
    }

    public async Task<Dictionary<string, object>> GetAnnualReportAsync(long userId, int year)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var startDate = new DateOnly(year, 1, 1);
        var endDate = new DateOnly(year, 12, 31);

        var moodTrends = await GetMoodTrendsAsync(userId, startDate, endDate);
        var activityPatterns = await GetActivityPatternsAsync(userId, startDate, endDate);
        var communicationStats = await GetCommunicationStatsAsync(userId, startDate, endDate);

        // Estatísticas de memórias
        var memories = await _context.Memories
            .Include(m => m.Media)
            .Include(m => m.Tags)
            .Where(m => m.RelationshipId == relationship.Id && 
                       m.MemoryDate >= startDate && 
                       m.MemoryDate <= endDate && 
                       m.DeletedAt == null)
            .ToListAsync();

        // Estatísticas de desejos
        var wishes = await _context.Wishes
            .Include(w => w.Category)
            .Where(w => w.RelationshipId == relationship.Id && 
                       w.CreatedAt.Year == year && 
                       w.DeletedAt == null)
            .ToListAsync();

        var report = new Dictionary<string, object>
        {
            { "year", year },
            { "relationship", new
                {
                    daysTogether = relationship.RelationshipStartDate.HasValue 
                        ? (DateOnly.FromDateTime(DateTime.Today).DayNumber - relationship.RelationshipStartDate.Value.DayNumber)
                        : 0,
                    relationshipStartDate = relationship.RelationshipStartDate?.ToString("yyyy-MM-dd"),
                    metDate = relationship.MetDate?.ToString("yyyy-MM-dd")
                }
            },
            { "moodTrends", moodTrends },
            { "activityPatterns", activityPatterns },
            { "communicationStats", communicationStats },
            { "memories", new
                {
                    total = memories.Count,
                    totalMedia = memories.Sum(m => m.Media.Count),
                    byMonth = memories.GroupBy(m => m.MemoryDate.Month)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count())
                }
            },
            { "wishes", new
                {
                    total = wishes.Count,
                    fulfilled = wishes.Count(w => w.Status == WishStatus.Fulfilled),
                    pending = wishes.Count(w => w.Status == WishStatus.Pending),
                    byCategory = wishes.GroupBy(w => w.Category?.Name ?? "Sem categoria")
                        .ToDictionary(g => g.Key, g => g.Count())
                }
            }
        };

        return report;
    }

    public async Task<Dictionary<string, object>> GetMonthlyReportAsync(long userId, int year, int month)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var moodTrends = await GetMoodTrendsAsync(userId, startDate, endDate);
        var activityPatterns = await GetActivityPatternsAsync(userId, startDate, endDate);
        var communicationStats = await GetCommunicationStatsAsync(userId, startDate, endDate);

        var memories = await _context.Memories
            .Include(m => m.Media)
            .Where(m => m.RelationshipId == relationship.Id && 
                       m.MemoryDate >= startDate && 
                       m.MemoryDate <= endDate && 
                       m.DeletedAt == null)
            .ToListAsync();

        var wishes = await _context.Wishes
            .Where(w => w.RelationshipId == relationship.Id && 
                       w.CreatedAt.Year == year && 
                       w.CreatedAt.Month == month && 
                       w.DeletedAt == null)
            .ToListAsync();

        var report = new Dictionary<string, object>
        {
            { "year", year },
            { "month", month },
            { "moodTrends", moodTrends },
            { "activityPatterns", activityPatterns },
            { "communicationStats", communicationStats },
            { "memories", new
                {
                    total = memories.Count,
                    totalMedia = memories.Sum(m => m.Media.Count)
                }
            },
            { "wishes", new
                {
                    total = wishes.Count,
                    fulfilled = wishes.Count(w => w.Status == WishStatus.Fulfilled),
                    pending = wishes.Count(w => w.Status == WishStatus.Pending)
                }
            }
        };

        return report;
    }

    private async Task<Domain.Models.Entities.Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }
}


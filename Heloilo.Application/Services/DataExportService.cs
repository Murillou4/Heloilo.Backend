using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class DataExportService : IDataExportService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<DataExportService> _logger;

    public DataExportService(HeloiloDbContext context, ILogger<DataExportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<string> ExportUserDataAsJsonAsync(long userId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

        if (user == null)
        {
            throw new KeyNotFoundException("Usuário não encontrado");
        }

        var exportData = new
        {
            exportedAt = DateTime.UtcNow,
            user = new
            {
                id = user.Id,
                email = user.Email,
                name = user.Name,
                nickname = user.Nickname,
                themeColor = user.ThemeColor,
                isActive = user.IsActive,
                emailVerified = user.EmailVerified,
                createdAt = user.CreatedAt,
                updatedAt = user.UpdatedAt,
                hasProfilePhoto = user.ProfilePhotoBlob != null && user.ProfilePhotoBlob.Length > 0
            },
            relationships = await GetRelationshipsAsync(userId),
            wishes = await GetWishesAsync(userId),
            memories = await GetMemoriesAsync(userId),
            chatMessages = await GetChatMessagesAsync(userId),
            moodLogs = await GetMoodLogsAsync(userId),
            activities = await GetActivitiesAsync(userId),
            userStatuses = await GetUserStatusesAsync(userId),
            notifications = await GetNotificationsAsync(userId),
            storyPages = await GetStoryPagesAsync(userId),
            favorites = await GetFavoritesAsync(userId),
            sharedContent = await GetSharedContentAsync(userId)
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(exportData, options);
    }

    public async Task<byte[]> ExportUserDataAsPdfAsync(long userId)
    {
        // Por enquanto, retornamos JSON como PDF (pode ser melhorado com biblioteca de PDF)
        var jsonData = await ExportUserDataAsJsonAsync(userId);
        return System.Text.Encoding.UTF8.GetBytes(jsonData);
    }

    private async Task<List<object>> GetRelationshipsAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.DeletedAt == null)
            .ToListAsync();

        return relationships.Select(r => new
        {
            id = r.Id,
            user1Id = r.User1Id,
            user1Name = r.User1.Name,
            user2Id = r.User2Id,
            user2Name = r.User2.Name,
            metDate = r.MetDate?.ToString("yyyy-MM-dd"),
            metLocation = r.MetLocation,
            relationshipStartDate = r.RelationshipStartDate?.ToString("yyyy-MM-dd"),
            celebrationType = r.CelebrationType.ToString(),
            isActive = r.IsActive,
            createdAt = r.CreatedAt,
            updatedAt = r.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetWishesAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .Select(r => r.Id)
            .ToListAsync();

        var wishes = await _context.Wishes
            .Include(w => w.Category)
            .Include(w => w.Comments)
            .Where(w => relationships.Contains(w.RelationshipId) && w.DeletedAt == null)
            .ToListAsync();

        return wishes.Select(w => new
        {
            id = w.Id,
            userId = w.UserId,
            relationshipId = w.RelationshipId,
            categoryId = w.CategoryId,
            categoryName = w.Category?.Name,
            title = w.Title,
            description = w.Description,
            linkUrl = w.LinkUrl,
            hasImage = w.ImageBlob != null && w.ImageBlob.Length > 0,
            importanceLevel = w.ImportanceLevel,
            status = w.Status.ToString(),
            createdAt = w.CreatedAt,
            updatedAt = w.UpdatedAt,
            comments = w.Comments.Where(c => c.DeletedAt == null).Select(c => new
            {
                id = c.Id,
                userId = c.UserId,
                content = c.Content,
                createdAt = c.CreatedAt,
                updatedAt = c.UpdatedAt
            }).ToList()
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetMemoriesAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .Select(r => r.Id)
            .ToListAsync();

        var memories = await _context.Memories
            .Include(m => m.Media)
            .Include(m => m.Tags)
            .Where(m => relationships.Contains(m.RelationshipId) && m.DeletedAt == null)
            .ToListAsync();

        return memories.Select(m => new
        {
            id = m.Id,
            relationshipId = m.RelationshipId,
            title = m.Title,
            description = m.Description,
            memoryDate = m.MemoryDate.ToString("yyyy-MM-dd"),
            createdAt = m.CreatedAt,
            updatedAt = m.UpdatedAt,
            mediaCount = m.Media.Count,
            tags = m.Tags.Select(t => t.TagName).ToList()
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetChatMessagesAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .Select(r => r.Id)
            .ToListAsync();

        var messages = await _context.ChatMessages
            .Include(m => m.Media)
            .Where(m => relationships.Contains(m.RelationshipId) && m.DeletedAt == null)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return messages.Select(m => new
        {
            id = m.Id,
            relationshipId = m.RelationshipId,
            senderId = m.SenderId,
            content = m.Content,
            messageType = m.MessageType.ToString(),
            deliveryStatus = m.DeliveryStatus.ToString(),
            sentAt = m.SentAt,
            deliveredAt = m.DeliveredAt,
            readAt = m.ReadAt,
            createdAt = m.CreatedAt,
            hasMedia = m.Media.Any(),
            mediaCount = m.Media.Count
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetMoodLogsAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .Select(r => r.Id)
            .ToListAsync();

        var moodLogs = await _context.MoodLogs
            .Include(m => m.MoodType)
            .Where(m => m.UserId == userId && relationships.Contains(m.RelationshipId))
            .OrderBy(m => m.LogDate)
            .ToListAsync();

        return moodLogs.Select(m => new
        {
            id = m.Id,
            userId = m.UserId,
            relationshipId = m.RelationshipId,
            moodTypeId = m.MoodTypeId,
            moodTypeName = m.MoodType.Name,
            moodCategory = m.MoodType.MoodCategory.ToString(),
            comment = m.Comment,
            logDate = m.LogDate.ToString("yyyy-MM-dd"),
            createdAt = m.CreatedAt,
            updatedAt = m.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetActivitiesAsync(long userId)
    {
        var activities = await _context.DailyActivities
            .Where(a => a.UserId == userId && a.DeletedAt == null)
            .OrderBy(a => a.ActivityDate)
            .ToListAsync();

        return activities.Select(a => new
        {
            id = a.Id,
            userId = a.UserId,
            title = a.Title,
            description = a.Description,
            isCompleted = a.IsCompleted,
            reminderMinutes = a.ReminderMinutes,
            activityDate = a.ActivityDate.ToString("yyyy-MM-dd"),
            createdAt = a.CreatedAt,
            updatedAt = a.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetUserStatusesAsync(long userId)
    {
        var statuses = await _context.UserStatuses
            .Where(s => s.UserId == userId)
            .OrderBy(s => s.StatusUpdatedAt)
            .ToListAsync();

        return statuses.Select(s => new
        {
            id = s.Id,
            userId = s.UserId,
            currentStatus = s.CurrentStatus,
            statusUpdatedAt = s.StatusUpdatedAt,
            createdAt = s.CreatedAt,
            updatedAt = s.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetNotificationsAsync(long userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.SentAt)
            .ToListAsync();

        return notifications.Select(n => new
        {
            id = n.Id,
            userId = n.UserId,
            relationshipId = n.RelationshipId,
            title = n.Title,
            content = n.Content,
            notificationType = n.NotificationType.ToString(),
            isRead = n.IsRead,
            sentAt = n.SentAt,
            readAt = n.ReadAt,
            createdAt = n.CreatedAt,
            updatedAt = n.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetStoryPagesAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .Select(r => r.Id)
            .ToListAsync();

        var storyPages = await _context.StoryPages
            .Where(s => relationships.Contains(s.RelationshipId) && s.DeletedAt == null)
            .OrderBy(s => s.PageNumber)
            .ToListAsync();

        return storyPages.Select(s => new
        {
            id = s.Id,
            relationshipId = s.RelationshipId,
            pageNumber = s.PageNumber,
            title = s.Title,
            content = s.Content,
            hasImage = s.ImageBlob != null && s.ImageBlob.Length > 0,
            pageDate = s.PageDate.ToString("yyyy-MM-dd"),
            createdAt = s.CreatedAt,
            updatedAt = s.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetFavoritesAsync(long userId)
    {
        var favorites = await _context.Favorites
            .Where(f => f.UserId == userId)
            .ToListAsync();

        return favorites.Select(f => new
        {
            id = f.Id,
            userId = f.UserId,
            contentType = f.ContentType.ToString(),
            contentId = f.ContentId,
            createdAt = f.CreatedAt,
            updatedAt = f.UpdatedAt
        }).Cast<object>().ToList();
    }

    private async Task<List<object>> GetSharedContentAsync(long userId)
    {
        var relationships = await _context.Relationships
            .Where(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null)
            .Select(r => r.Id)
            .ToListAsync();

        var sharedContent = await _context.SharedContents
            .Where(s => relationships.Contains(s.RelationshipId))
            .ToListAsync();

        return sharedContent.Select(s => new
        {
            id = s.Id,
            relationshipId = s.RelationshipId,
            contentType = s.ContentType.ToString(),
            contentId = s.ContentId,
            shareToken = s.Token,
            expiresAt = s.ExpiresAt,
            createdAt = s.CreatedAt,
            updatedAt = s.UpdatedAt
        }).Cast<object>().ToList();
    }
}


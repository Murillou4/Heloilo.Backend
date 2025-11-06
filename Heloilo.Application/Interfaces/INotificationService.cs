using Heloilo.Application.DTOs.Notification;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.Interfaces;

public interface INotificationService
{
    Task<PagedResult<NotificationDto>> GetNotificationsAsync(long userId, NotificationType? notificationType = null, bool? isRead = null, DateOnly? startDate = null, DateOnly? endDate = null, int page = 1, int pageSize = 20);
    Task<bool> MarkAsReadAsync(long notificationId, long userId);
    Task<bool> MarkAllAsReadAsync(long userId);
    Task<int> GetUnreadCountAsync(long userId);
    Task<List<NotificationPreferenceDto>> GetPreferencesAsync(long userId);
    Task<bool> UpdatePreferencesAsync(long userId, List<NotificationPreferenceDto> preferences);
    Task CreateAndSendNotificationAsync(long userId, long relationshipId, string title, string content, NotificationType notificationType);
}


using Heloilo.Application.DTOs.Notification;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Heloilo.Application.Hubs;

namespace Heloilo.Application.Services;

public class NotificationService : INotificationService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<NotificationService> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(HeloiloDbContext context, ILogger<NotificationService> logger, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _hubContext = hubContext;
    }

    public async Task<PagedResult<NotificationDto>> GetNotificationsAsync(long userId, NotificationType? notificationType = null, bool? isRead = null, DateOnly? startDate = null, DateOnly? endDate = null, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (notificationType.HasValue)
        {
            query = query.Where(n => n.NotificationType == notificationType.Value);
        }

        if (isRead.HasValue)
        {
            query = query.Where(n => n.IsRead == isRead.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(n => DateOnly.FromDateTime(n.SentAt) >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(n => DateOnly.FromDateTime(n.SentAt) <= endDate.Value);
        }

        var totalItems = await query.CountAsync();

        var notifications = await query
            .OrderByDescending(n => n.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<NotificationDto>
        {
            Items = notifications.Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<bool> MarkAsReadAsync(long notificationId, long userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) throw new KeyNotFoundException("Notificação não encontrada");

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Notificar via SignalR
        try
        {
            await _hubContext.Clients.Group($"user:{userId}")
                .SendAsync("NotificationRead", notificationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao notificar leitura de notificação via SignalR");
        }

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(long userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        // Notificar via SignalR
        try
        {
            await _hubContext.Clients.Group($"user:{userId}")
                .SendAsync("AllNotificationsRead");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao notificar leitura de todas as notificações via SignalR");
        }

        return true;
    }

    public async Task<int> GetUnreadCountAsync(long userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<List<NotificationPreferenceDto>> GetPreferencesAsync(long userId)
    {
        var preferences = await _context.NotificationPreferences
            .Where(np => np.UserId == userId)
            .ToListAsync();

        // Se não existir preferências, criar padrões
        if (!preferences.Any())
        {
            var defaultTypes = Enum.GetValues<NotificationType>();
            foreach (var type in defaultTypes)
            {
                var pref = new NotificationPreference
                {
                    UserId = userId,
                    NotificationType = type,
                    IsEnabled = true,
                    IntensityLevel = IntensityLevel.Normal
                };
                _context.NotificationPreferences.Add(pref);
            }
            await _context.SaveChangesAsync();
            preferences = await _context.NotificationPreferences
                .Where(np => np.UserId == userId)
                .ToListAsync();
        }

        return preferences.Select(MapPreferenceToDto).ToList();
    }

    public async Task<bool> UpdatePreferencesAsync(long userId, List<NotificationPreferenceDto> preferences)
    {
        foreach (var prefDto in preferences)
        {
            var pref = await _context.NotificationPreferences
                .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == prefDto.NotificationType);

            if (pref == null)
            {
                pref = new NotificationPreference
                {
                    UserId = userId,
                    NotificationType = prefDto.NotificationType
                };
                _context.NotificationPreferences.Add(pref);
            }

            pref.IsEnabled = prefDto.IsEnabled;
            pref.QuietStartTime = prefDto.QuietStartTime;
            pref.QuietEndTime = prefDto.QuietEndTime;
            pref.IntensityLevel = prefDto.IntensityLevel;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task CreateAndSendNotificationAsync(long userId, long relationshipId, string title, string content, NotificationType notificationType)
    {
        // Verificar se o usuário tem preferências habilitadas para este tipo
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(np => np.UserId == userId && np.NotificationType == notificationType);

        if (preference != null && !preference.IsEnabled)
        {
            return; // Notificação desabilitada pelo usuário
        }

        var notification = new Notification
        {
            UserId = userId,
            RelationshipId = relationshipId,
            Title = title,
            Content = content,
            NotificationType = notificationType,
            SentAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Enviar via SignalR
        try
        {
            var notificationDto = MapToDto(notification);
            await _hubContext.Clients.Group($"user:{userId}")
                .SendAsync("NotificationReceived", notificationDto);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao enviar notificação via SignalR");
        }
    }

    private static NotificationDto MapToDto(Notification notification)
    {
        return new NotificationDto
        {
            Id = notification.Id,
            Title = notification.Title,
            Content = notification.Content,
            NotificationType = notification.NotificationType,
            IsRead = notification.IsRead,
            SentAt = notification.SentAt,
            ReadAt = notification.ReadAt
        };
    }

    private static NotificationPreferenceDto MapPreferenceToDto(NotificationPreference preference)
    {
        return new NotificationPreferenceDto
        {
            NotificationType = preference.NotificationType,
            IsEnabled = preference.IsEnabled,
            QuietStartTime = preference.QuietStartTime,
            QuietEndTime = preference.QuietEndTime,
            IntensityLevel = preference.IntensityLevel
        };
    }
}


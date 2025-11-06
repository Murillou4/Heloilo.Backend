using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Heloilo.Application.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;
    private readonly HeloiloDbContext _context;

    public NotificationHub(ILogger<NotificationHub> logger, HeloiloDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{userId}");
            _logger.LogInformation("Usuário {UserId} conectado ao hub de notificações", userId);
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user:{userId}");
            _logger.LogInformation("Usuário {UserId} desconectado do hub de notificações", userId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendNotificationUpdate(object notification)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        await Clients.Group($"user:{userId}")
            .SendAsync("NotificationReceived", notification);
    }

    public async Task SendNotificationReadUpdate(long notificationId)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        await Clients.Group($"user:{userId}")
            .SendAsync("NotificationRead", notificationId);
    }

    private long? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userId))
        {
            return null;
        }
        return userId;
    }
}


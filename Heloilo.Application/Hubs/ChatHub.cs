using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Heloilo.Application.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly ILogger<ChatHub> _logger;
    private readonly HeloiloDbContext _context;

    public ChatHub(ILogger<ChatHub> logger, HeloiloDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            var relationshipId = await GetRelationshipIdAsync(userId.Value);
            if (relationshipId.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"relationship:{relationshipId}");
                _logger.LogInformation("Usuário {UserId} conectado ao chat do relacionamento {RelationshipId}", userId, relationshipId);
            }
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            var relationshipId = await GetRelationshipIdAsync(userId.Value);
            if (relationshipId.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"relationship:{relationshipId}");
                _logger.LogInformation("Usuário {UserId} desconectado do chat do relacionamento {RelationshipId}", userId, relationshipId);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTypingIndicator(bool isTyping)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        var relationshipId = await GetRelationshipIdAsync(userId.Value);
        if (!relationshipId.HasValue) return;

        await Clients.GroupExcept($"relationship:{relationshipId}", Context.ConnectionId)
            .SendAsync("PartnerTyping", isTyping);
    }

    public async Task SendMessageUpdate(object message)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        var relationshipId = await GetRelationshipIdAsync(userId.Value);
        if (!relationshipId.HasValue) return;

        await Clients.Group($"relationship:{relationshipId}")
            .SendAsync("MessageUpdated", message);
    }

    public async Task SendDeliveryStatus(long messageId, string status)
    {
        var userId = GetUserId();
        if (!userId.HasValue) return;

        var relationshipId = await GetRelationshipIdAsync(userId.Value);
        if (!relationshipId.HasValue) return;

        await Clients.Group($"relationship:{relationshipId}")
            .SendAsync("DeliveryStatusUpdated", new { messageId, status });
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

    private async Task<long?> GetRelationshipIdAsync(long userId)
    {
        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
        
        return relationship?.Id;
    }
}


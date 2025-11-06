using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DashboardController : BaseController
{
    private readonly IRelationshipService _relationshipService;
    private readonly IMemoryService _memoryService;
    private readonly IWishService _wishService;
    private readonly IMoodLogService _moodLogService;
    private readonly IActivityService _activityService;
    private readonly IChatService _chatService;
    private readonly INotificationService _notificationService;
    private readonly IUserStatusService _userStatusService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IRelationshipService relationshipService,
        IMemoryService memoryService,
        IWishService wishService,
        IMoodLogService moodLogService,
        IActivityService activityService,
        IChatService chatService,
        INotificationService notificationService,
        IUserStatusService userStatusService,
        ILogger<DashboardController> logger)
    {
        _relationshipService = relationshipService;
        _memoryService = memoryService;
        _wishService = wishService;
        _moodLogService = moodLogService;
        _activityService = activityService;
        _chatService = chatService;
        _notificationService = notificationService;
        _userStatusService = userStatusService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetDashboard()
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.GetCurrentRelationshipAsync(userId);
            
            if (relationship == null)
            {
                return RouteMessages.BadRequest("Nenhum relacionamento ativo encontrado", "Relacionamento não encontrado");
            }

            // Buscar estatísticas em paralelo
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            
            var memoriesTask = _memoryService.GetMemoriesAsync(userId, null, null, null, null, null, null, 1, 1);
            var wishesTask = _wishService.GetWishesAsync(userId, null, null, null, null, 1, 1);
            var todayActivitiesTask = _activityService.GetActivitiesAsync(userId, today, null, null, null, null, null, null, 1, 100);
            var upcomingActivitiesTask = _activityService.GetActivitiesAsync(userId, null, tomorrow, null, false, null, null, null, 1, 5);
            var messagesTask = _chatService.GetMessagesAsync(userId, null, null, null, null, null, 1, 1);
            var unreadMessagesTask = _chatService.GetMessagesAsync(userId, null, DeliveryStatus.Sent, null, null, null, 1, 1);
            var unreadNotificationsTask = _notificationService.GetUnreadCountAsync(userId);
            var recentMoodLogsTask = _moodLogService.GetMoodLogsAsync(userId, null, null, null, null, null, null, 1, 5);
            var partnerStatusTask = _userStatusService.GetPartnerStatusAsync(userId);

            await Task.WhenAll(memoriesTask, wishesTask, todayActivitiesTask, upcomingActivitiesTask, 
                               messagesTask, unreadMessagesTask, unreadNotificationsTask, recentMoodLogsTask, partnerStatusTask);

            var memories = await memoriesTask;
            var wishes = await wishesTask;
            var todayActivities = await todayActivitiesTask;
            var upcomingActivities = await upcomingActivitiesTask;
            var messages = await messagesTask;
            var unreadMessages = await unreadMessagesTask;
            var unreadNotifications = await unreadNotificationsTask;
            var recentMoodLogs = await recentMoodLogsTask;
            var partnerStatus = await partnerStatusTask;

            var data = new Dictionary<string, object>
            {
                { "relationship", new Dictionary<string, object>
                    {
                        { "daysTogether", relationship.DaysTogether },
                        { "relationshipStartDate", relationship.RelationshipStartDate?.ToString("yyyy-MM-dd") ?? "" },
                        { "metDate", relationship.MetDate?.ToString("yyyy-MM-dd") ?? "" },
                        { "metLocation", relationship.MetLocation ?? "" }
                    }
                },
                { "statistics", new Dictionary<string, object>
                    {
                        { "totalMemories", memories.TotalItems },
                        { "totalWishes", wishes.TotalItems },
                        { "totalMessages", messages.TotalItems },
                        { "unreadMessages", unreadMessages.TotalItems },
                        { "unreadNotifications", unreadNotifications },
                        { "todayActivities", todayActivities.Items.Count },
                        { "upcomingActivities", upcomingActivities.Items.Count }
                    }
                },
                { "recentMoodLogs", recentMoodLogs.Items },
                { "upcomingActivities", upcomingActivities.Items },
                { "partnerStatus", partnerStatus != null ? (object)new Dictionary<string, object>
                    {
                        { "currentStatus", partnerStatus.CurrentStatus ?? "" },
                        { "statusUpdatedAt", partnerStatus.StatusUpdatedAt },
                        { "isExpired", partnerStatus.IsExpired }
                    } : new Dictionary<string, object>()
                }
            };

            return RouteMessages.Ok("Dashboard obtido com sucesso", "Dashboard", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dashboard");
            return RouteMessages.InternalError("Erro ao obter dashboard", "Erro interno");
        }
    }
}


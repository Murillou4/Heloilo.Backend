using Heloilo.Application.DTOs.Notification;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class NotificationsController : BaseController
{
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetNotifications([FromQuery] NotificationType? notificationType, [FromQuery] bool? isRead, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationService.GetNotificationsAsync(userId, notificationType, isRead, startDate, endDate, page, pageSize);
            return RouteMessages.OkPaged("notifications", notifications, "Notificações listadas com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar notificações");
            return RouteMessages.InternalError("Erro ao listar notificações", "Erro interno");
        }
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult> MarkAsRead(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAsReadAsync(id, userId);
            return RouteMessages.Ok("Notificação marcada como lida", "Notificação atualizada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Notificação não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar notificação como lida");
            return RouteMessages.InternalError("Erro ao marcar notificação como lida", "Erro interno");
        }
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = GetCurrentUserId();
            await _notificationService.MarkAllAsReadAsync(userId);
            return RouteMessages.Ok("Todas as notificações foram marcadas como lidas", "Notificações atualizadas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar todas as notificações como lidas");
            return RouteMessages.InternalError("Erro ao marcar notificações como lidas", "Erro interno");
        }
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var count = await _notificationService.GetUnreadCountAsync(userId);
            var data = new Dictionary<string, object> { { "count", count } };
            return RouteMessages.Ok("Contagem obtida com sucesso", "Contagem de não lidas", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de não lidas");
            return RouteMessages.InternalError("Erro ao obter contagem", "Erro interno");
        }
    }

    [HttpGet("preferences")]
    public async Task<ActionResult> GetPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            var preferences = await _notificationService.GetPreferencesAsync(userId);
            var data = new Dictionary<string, object> { { "preferences", preferences } };
            return RouteMessages.Ok("Preferências obtidas com sucesso", "Preferências", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter preferências");
            return RouteMessages.InternalError("Erro ao obter preferências", "Erro interno");
        }
    }

    [HttpPut("preferences")]
    public async Task<ActionResult> UpdatePreferences([FromBody] List<NotificationPreferenceDto> preferences)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            await _notificationService.UpdatePreferencesAsync(userId, preferences);
            return RouteMessages.Ok("Preferências atualizadas com sucesso", "Preferências atualizadas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar preferências");
            return RouteMessages.InternalError("Erro ao atualizar preferências", "Erro interno");
        }
    }

}

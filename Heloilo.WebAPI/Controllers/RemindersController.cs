using Heloilo.Application.DTOs.Reminder;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class RemindersController : BaseController
{
    private readonly IReminderService _reminderService;
    private readonly ILogger<RemindersController> _logger;

    public RemindersController(IReminderService reminderService, ILogger<RemindersController> logger)
    {
        _reminderService = reminderService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetReminders([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] bool? isCompleted, [FromQuery] bool? isActive, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reminders = await _reminderService.GetRemindersAsync(userId, startDate, endDate, isCompleted, isActive, page, pageSize);
            return RouteMessages.OkPaged("reminders", reminders, "Lembretes listados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar lembretes");
            return RouteMessages.InternalError("Erro ao listar lembretes", "Erro interno");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetReminder(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reminder = await _reminderService.GetReminderByIdAsync(id, userId);
            var data = new Dictionary<string, object> { { "reminder", reminder } };
            return RouteMessages.Ok("Lembrete obtido com sucesso", "Lembrete encontrado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Lembrete não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter lembrete");
            return RouteMessages.InternalError("Erro ao obter lembrete", "Erro interno");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateReminder([FromBody] CreateReminderDto dto)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var userId = GetCurrentUserId();
            var reminder = await _reminderService.CreateReminderAsync(userId, dto);
            var data = new Dictionary<string, object> { { "reminder", reminder } };
            return RouteMessages.Ok("Lembrete criado com sucesso", "Lembrete criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar lembrete");
            return RouteMessages.InternalError("Erro ao criar lembrete", "Erro interno");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateReminder(long id, [FromBody] UpdateReminderDto dto)
    {
        try
        {
            var validationError = ValidateModelState();
            if (validationError != null) return validationError;

            var userId = GetCurrentUserId();
            var reminder = await _reminderService.UpdateReminderAsync(id, userId, dto);
            var data = new Dictionary<string, object> { { "reminder", reminder } };
            return RouteMessages.Ok("Lembrete atualizado com sucesso", "Lembrete atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Lembrete não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar lembrete");
            return RouteMessages.InternalError("Erro ao atualizar lembrete", "Erro interno");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReminder(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _reminderService.DeleteReminderAsync(id, userId);
            return RouteMessages.Ok("Lembrete excluído com sucesso", "Lembrete excluído");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Lembrete não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir lembrete");
            return RouteMessages.InternalError("Erro ao excluir lembrete", "Erro interno");
        }
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult> MarkAsCompleted(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var reminder = await _reminderService.MarkAsCompletedAsync(id, userId);
            var data = new Dictionary<string, object> { { "reminder", reminder } };
            return RouteMessages.Ok("Lembrete marcado como concluído com sucesso", "Lembrete atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Lembrete não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar lembrete como concluído");
            return RouteMessages.InternalError("Erro ao marcar lembrete como concluído", "Erro interno");
        }
    }
}


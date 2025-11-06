using Heloilo.Application.DTOs.Activity;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class DailyActivitiesController : BaseController
{
    private readonly IActivityService _activityService;
    private readonly ILogger<DailyActivitiesController> _logger;

    public DailyActivitiesController(IActivityService activityService, ILogger<DailyActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetActivities([FromQuery] DateOnly? date, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] bool? isCompleted, [FromQuery] bool? hasReminder, [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var activities = await _activityService.GetActivitiesAsync(userId, date, startDate, endDate, isCompleted, hasReminder, sortBy, sortOrder, page, pageSize);
            return RouteMessages.OkPaged("activities", activities, "Atividades listadas com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar atividades");
            return RouteMessages.InternalError("Erro ao listar atividades", "Erro interno");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetActivity(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var activity = await _activityService.GetActivityByIdAsync(id, userId);
            var data = new Dictionary<string, object> { { "activity", activity } };
            return RouteMessages.Ok("Atividade obtida com sucesso", "Atividade encontrada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Atividade não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter atividade");
            return RouteMessages.InternalError("Erro ao obter atividade", "Erro interno");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateActivity([FromBody] CreateActivityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var activity = await _activityService.CreateActivityAsync(userId, dto);
            var data = new Dictionary<string, object> { { "activity", activity } };
            return RouteMessages.Ok("Atividade criada com sucesso", "Atividade criada", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar atividade");
            return RouteMessages.InternalError("Erro ao criar atividade", "Erro interno");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateActivity(long id, [FromBody] CreateActivityDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var activity = await _activityService.UpdateActivityAsync(id, userId, dto);
            var data = new Dictionary<string, object> { { "activity", activity } };
            return RouteMessages.Ok("Atividade atualizada com sucesso", "Atividade atualizada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Atividade não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar atividade");
            return RouteMessages.InternalError("Erro ao atualizar atividade", "Erro interno");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteActivity(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _activityService.DeleteActivityAsync(id, userId);
            return RouteMessages.Ok("Atividade excluída com sucesso", "Atividade excluída");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Atividade não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir atividade");
            return RouteMessages.InternalError("Erro ao excluir atividade", "Erro interno");
        }
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult> MarkAsCompleted(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var activity = await _activityService.MarkAsCompletedAsync(id, userId);
            var data = new Dictionary<string, object> { { "activity", activity } };
            return RouteMessages.Ok("Atividade marcada como concluída", "Atividade atualizada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Atividade não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar atividade como concluída");
            return RouteMessages.InternalError("Erro ao marcar atividade como concluída", "Erro interno");
        }
    }

    [HttpGet("partner")]
    public async Task<ActionResult> GetPartnerActivities([FromQuery] DateOnly? date, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var activities = await _activityService.GetPartnerActivitiesAsync(userId, date, page, pageSize);
            return RouteMessages.OkPaged("activities", activities, "Atividades do parceiro listadas com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter atividades do parceiro");
            return RouteMessages.InternalError("Erro ao obter atividades do parceiro", "Erro interno");
        }
    }
}

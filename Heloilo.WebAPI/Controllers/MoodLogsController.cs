using Heloilo.Application.DTOs.MoodLog;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MoodLogsController : BaseController
{
    private readonly IMoodLogService _moodLogService;
    private readonly ILogger<MoodLogsController> _logger;

    public MoodLogsController(IMoodLogService moodLogService, ILogger<MoodLogsController> logger)
    {
        _moodLogService = moodLogService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> CreateMoodLog([FromBody] CreateMoodLogDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var moodLog = await _moodLogService.CreateMoodLogAsync(userId, dto);
            var data = new Dictionary<string, object> { { "moodLog", moodLog } };
            return RouteMessages.Ok("Humor registrado com sucesso", "Humor registrado", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar humor");
            return RouteMessages.InternalError("Erro ao registrar humor", "Erro interno");
        }
    }

    [HttpGet]
    public async Task<ActionResult> GetMoodLogs([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] long? moodTypeId, [FromQuery] string? moodCategory, [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var logs = await _moodLogService.GetMoodLogsAsync(userId, startDate, endDate, moodTypeId, moodCategory, sortBy, sortOrder, page, pageSize);
            return RouteMessages.OkPaged("moodLogs", logs, "Registros de humor listados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar registros de humor");
            return RouteMessages.InternalError("Erro ao listar registros de humor", "Erro interno");
        }
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult> GetDashboard([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            var dashboard = await _moodLogService.GetDashboardAsync(userId, startDate, endDate);
            var data = new Dictionary<string, object> { { "dashboard", dashboard } };
            return RouteMessages.Ok("Dashboard obtido com sucesso", "Dashboard", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dashboard");
            return RouteMessages.InternalError("Erro ao obter dashboard", "Erro interno");
        }
    }

    [HttpGet("partner")]
    public async Task<ActionResult> GetPartnerMoodLogs([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var logs = await _moodLogService.GetPartnerMoodLogsAsync(userId, startDate, endDate, page, pageSize);
            return RouteMessages.OkPaged("moodLogs", logs, "Humor do parceiro listado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter humor do parceiro");
            return RouteMessages.InternalError("Erro ao obter humor do parceiro", "Erro interno");
        }
    }

    [HttpGet("today")]
    public async Task<ActionResult> GetTodayTimeline()
    {
        try
        {
            var userId = GetCurrentUserId();
            var logs = await _moodLogService.GetTodayTimelineAsync(userId);
            var data = new Dictionary<string, object> { { "moodLogs", logs } };
            return RouteMessages.Ok("Timeline do dia obtida com sucesso", "Timeline do dia", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter timeline do dia");
            return RouteMessages.InternalError("Erro ao obter timeline do dia", "Erro interno");
        }
    }

    [HttpGet("types")]
    public async Task<ActionResult> GetMoodTypes()
    {
        try
        {
            var types = await _moodLogService.GetMoodTypesAsync();
            var data = new Dictionary<string, object> { { "types", types } };
            return RouteMessages.Ok("Tipos de humor listados com sucesso", "Tipos de humor", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tipos de humor");
            return RouteMessages.InternalError("Erro ao listar tipos de humor", "Erro interno");
        }
    }
}

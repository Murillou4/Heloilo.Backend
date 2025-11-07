using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AnalyticsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("mood-trends")]
    public async Task<ActionResult> GetMoodTrends([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            var trends = await _analyticsService.GetMoodTrendsAsync(userId, startDate, endDate);
            var data = new Dictionary<string, object> { { "trends", trends } };
            return RouteMessages.Ok("Tendências de humor obtidas com sucesso", "Tendências de humor", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter tendências de humor");
            return RouteMessages.InternalError("Erro ao obter tendências de humor", "Erro interno");
        }
    }

    [HttpGet("activity-patterns")]
    public async Task<ActionResult> GetActivityPatterns([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            var patterns = await _analyticsService.GetActivityPatternsAsync(userId, startDate, endDate);
            var data = new Dictionary<string, object> { { "patterns", patterns } };
            return RouteMessages.Ok("Padrões de atividades obtidos com sucesso", "Padrões de atividades", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter padrões de atividades");
            return RouteMessages.InternalError("Erro ao obter padrões de atividades", "Erro interno");
        }
    }

    [HttpGet("communication-stats")]
    public async Task<ActionResult> GetCommunicationStats([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate)
    {
        try
        {
            var userId = GetCurrentUserId();
            var stats = await _analyticsService.GetCommunicationStatsAsync(userId, startDate, endDate);
            var data = new Dictionary<string, object> { { "stats", stats } };
            return RouteMessages.Ok("Estatísticas de comunicação obtidas com sucesso", "Estatísticas de comunicação", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de comunicação");
            return RouteMessages.InternalError("Erro ao obter estatísticas de comunicação", "Erro interno");
        }
    }
}


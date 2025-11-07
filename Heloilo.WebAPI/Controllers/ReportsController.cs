using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ReportsController : BaseController
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IAnalyticsService analyticsService, ILogger<ReportsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    [HttpGet("annual")]
    public async Task<ActionResult> GetAnnualReport([FromQuery] int year)
    {
        try
        {
            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return RouteMessages.BadRequest("Ano inválido", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var report = await _analyticsService.GetAnnualReportAsync(userId, year);
            var data = new Dictionary<string, object> { { "report", report } };
            return RouteMessages.Ok("Relatório anual obtido com sucesso", "Relatório anual", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relatório anual");
            return RouteMessages.InternalError("Erro ao obter relatório anual", "Erro interno");
        }
    }

    [HttpGet("monthly")]
    public async Task<ActionResult> GetMonthlyReport([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            if (year < 2000 || year > DateTime.Now.Year + 1)
            {
                return RouteMessages.BadRequest("Ano inválido", "Erro de validação");
            }

            if (month < 1 || month > 12)
            {
                return RouteMessages.BadRequest("Mês inválido. Deve estar entre 1 e 12", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var report = await _analyticsService.GetMonthlyReportAsync(userId, year, month);
            var data = new Dictionary<string, object> { { "report", report } };
            return RouteMessages.Ok("Relatório mensal obtido com sucesso", "Relatório mensal", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relatório mensal");
            return RouteMessages.InternalError("Erro ao obter relatório mensal", "Erro interno");
        }
    }
}


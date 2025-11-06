using Heloilo.Application.DTOs.Celebration;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class CelebrationController : BaseController
{
    private readonly ICelebrationService _celebrationService;
    private readonly ILogger<CelebrationController> _logger;

    public CelebrationController(ICelebrationService celebrationService, ILogger<CelebrationController> logger)
    {
        _celebrationService = celebrationService;
        _logger = logger;
    }

    [HttpGet("anniversary")]
    public async Task<ActionResult> GetAnniversaryInfo()
    {
        try
        {
            var userId = GetCurrentUserId();
            var info = await _celebrationService.GetAnniversaryInfoAsync(userId);
            var data = new Dictionary<string, object> { { "anniversaryInfo", info } };
            return RouteMessages.Ok("Informações de aniversário obtidas com sucesso", "Aniversário", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações de aniversário");
            return RouteMessages.InternalError("Erro ao obter informações de aniversário", "Erro interno");
        }
    }

    [HttpGet("upcoming")]
    public async Task<ActionResult> GetUpcomingCelebrations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var celebrations = await _celebrationService.GetUpcomingCelebrationsAsync(userId);
            var data = new Dictionary<string, object> { { "celebrations", celebrations } };
            return RouteMessages.Ok("Próximas comemorações obtidas com sucesso", "Comemorações", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter próximas comemorações");
            return RouteMessages.InternalError("Erro ao obter próximas comemorações", "Erro interno");
        }
    }
}

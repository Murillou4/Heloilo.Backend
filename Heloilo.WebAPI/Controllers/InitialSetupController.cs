using Heloilo.Application.DTOs.Relationship;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class InitialSetupController : BaseController
{
    private readonly IRelationshipService _relationshipService;
    private readonly ILogger<InitialSetupController> _logger;

    public InitialSetupController(IRelationshipService relationshipService, ILogger<InitialSetupController> logger)
    {
        _relationshipService = relationshipService;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<ActionResult> GetStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _relationshipService.GetInitialSetupStatusAsync(userId);
            var data = new Dictionary<string, object> { { "status", status } };
            return RouteMessages.Ok("Status da configuração inicial obtido com sucesso", "Status", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status da configuração inicial");
            return RouteMessages.InternalError("Erro ao obter status da configuração inicial", "Erro interno");
        }
    }

    [HttpPost("complete")]
    public async Task<ActionResult> Complete()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _relationshipService.CompleteInitialSetupAsync(userId);
            return RouteMessages.Ok("Configuração inicial completada com sucesso", "Configuração completada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao completar configuração inicial");
            return RouteMessages.InternalError("Erro ao completar configuração inicial", "Erro interno");
        }
    }

    [HttpPost("skip")]
    public async Task<ActionResult> Skip()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _relationshipService.SkipInitialSetupAsync(userId);
            return RouteMessages.Ok("Configuração inicial pulada com sucesso", "Configuração pulada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao pular configuração inicial");
            return RouteMessages.InternalError("Erro ao pular configuração inicial", "Erro interno");
        }
    }
}

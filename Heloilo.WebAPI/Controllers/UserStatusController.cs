using Heloilo.Application.DTOs.Status;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class UserStatusController : BaseController
{
    private readonly IUserStatusService _statusService;
    private readonly ILogger<UserStatusController> _logger;

    public UserStatusController(IUserStatusService statusService, ILogger<UserStatusController> logger)
    {
        _statusService = statusService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o status atual do usuário autenticado
    /// </summary>
    /// <returns>Status atual do usuário</returns>
    /// <response code="200">Status obtido com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet("current")]
    public async Task<ActionResult> GetCurrentStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _statusService.GetCurrentStatusAsync(userId);
            var data = new Dictionary<string, object> { { "status", status } };
            return RouteMessages.Ok("Status atual obtido com sucesso", "Status", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status atual");
            return RouteMessages.InternalError("Erro ao obter status atual", "Erro interno");
        }
    }

    /// <summary>
    /// Atualiza o status atual do usuário
    /// </summary>
    /// <param name="dto">Novo status do usuário</param>
    /// <returns>Status atualizado</returns>
    /// <response code="200">Status atualizado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPut]
    public async Task<ActionResult> UpdateStatus([FromBody] UpdateStatusDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var status = await _statusService.UpdateStatusAsync(userId, dto);
            var data = new Dictionary<string, object> { { "status", status } };
            return RouteMessages.Ok("Status atualizado com sucesso", "Status atualizado", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status");
            return RouteMessages.InternalError("Erro ao atualizar status", "Erro interno");
        }
    }

    [HttpGet("partner")]
    public async Task<ActionResult> GetPartnerStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _statusService.GetPartnerStatusAsync(userId);
            if (status == null)
            {
                return RouteMessages.BadRequest("Status do parceiro não encontrado", "Recurso não encontrado");
            }
            var data = new Dictionary<string, object> { { "status", status } };
            return RouteMessages.Ok("Status do parceiro obtido com sucesso", "Status do parceiro", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status do parceiro");
            return RouteMessages.InternalError("Erro ao obter status do parceiro", "Erro interno");
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult> GetStatusHistory([FromQuery] DateOnly? date, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var history = await _statusService.GetStatusHistoryAsync(userId, date, page, pageSize);
            return RouteMessages.OkPaged("statusHistory", history, "Histórico de status listado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter histórico de status");
            return RouteMessages.InternalError("Erro ao obter histórico de status", "Erro interno");
        }
    }

    [HttpGet("partner/history")]
    public async Task<ActionResult> GetPartnerStatusHistory([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var history = await _statusService.GetPartnerStatusHistoryAsync(userId, startDate, endDate, page, pageSize);
            return RouteMessages.OkPaged("statusHistory", history, "Histórico de status do parceiro listado com sucesso");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter histórico de status do parceiro");
            return RouteMessages.InternalError("Erro ao obter histórico de status do parceiro", "Erro interno");
        }
    }

    [HttpGet("expired")]
    public async Task<ActionResult> IsStatusExpired()
    {
        try
        {
            var userId = GetCurrentUserId();
            var isExpired = await _statusService.IsStatusExpiredAsync(userId);
            var data = new Dictionary<string, object> { { "isExpired", isExpired } };
            return RouteMessages.Ok("Status verificado com sucesso", "Status", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar expiração do status");
            return RouteMessages.InternalError("Erro ao verificar expiração do status", "Erro interno");
        }
    }

    [HttpPost("clear")]
    public async Task<ActionResult> ClearExpiredStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var status = await _statusService.ClearExpiredStatusAsync(userId);
            var data = new Dictionary<string, object> { { "status", status } };
            return RouteMessages.Ok("Status limpo com sucesso", "Status atualizado", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar status expirado");
            return RouteMessages.InternalError("Erro ao limpar status", "Erro interno");
        }
    }
}

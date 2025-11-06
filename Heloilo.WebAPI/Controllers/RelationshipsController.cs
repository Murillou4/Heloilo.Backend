using Heloilo.Application.DTOs.Relationship;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class RelationshipsController : BaseController
{
    private readonly IRelationshipService _relationshipService;
    private readonly ILogger<RelationshipsController> _logger;

    public RelationshipsController(IRelationshipService relationshipService, ILogger<RelationshipsController> logger)
    {
        _relationshipService = relationshipService;
        _logger = logger;
    }

    [HttpPost("invite")]
    public async Task<ActionResult> SendInvitation([FromBody] CreateRelationshipInvitationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var invitation = await _relationshipService.SendInvitationAsync(userId, dto);
            var data = new Dictionary<string, object> { { "invitation", invitation } };
            return RouteMessages.Ok("Convite enviado com sucesso", "Convite enviado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Recurso não encontrado");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Operação inválida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar convite");
            return RouteMessages.InternalError("Erro ao enviar convite", "Erro interno");
        }
    }

    [HttpGet("invitations")]
    public async Task<ActionResult> GetPendingInvitations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var invitations = await _relationshipService.GetPendingInvitationsAsync(userId, page, pageSize);
            return RouteMessages.OkPaged("invitations", invitations, "Convites listados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter convites");
            return RouteMessages.InternalError("Erro ao obter convites", "Erro interno");
        }
    }

    [HttpPost("invitations/{id}/accept")]
    public async Task<ActionResult> AcceptInvitation(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.AcceptInvitationAsync(userId, id);
            var data = new Dictionary<string, object> { { "relationship", relationship } };
            return RouteMessages.Ok("Convite aceito com sucesso", "Relacionamento criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Convite não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (InvalidOperationException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Operação inválida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aceitar convite");
            return RouteMessages.InternalError("Erro ao aceitar convite", "Erro interno");
        }
    }

    [HttpPost("invitations/{id}/reject")]
    public async Task<ActionResult> RejectInvitation(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _relationshipService.RejectInvitationAsync(userId, id);
            return RouteMessages.Ok("Convite rejeitado com sucesso", "Convite rejeitado");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Convite não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao rejeitar convite");
            return RouteMessages.InternalError("Erro ao rejeitar convite", "Erro interno");
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult> GetCurrentRelationship()
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.GetCurrentRelationshipAsync(userId);
            if (relationship == null)
            {
                return RouteMessages.BadRequest("Nenhum relacionamento ativo encontrado", "Relacionamento não encontrado");
            }
            var data = new Dictionary<string, object> { { "relationship", relationship } };
            return RouteMessages.Ok("Relacionamento obtido com sucesso", "Relacionamento encontrado", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relacionamento atual");
            return RouteMessages.InternalError("Erro ao obter relacionamento", "Erro interno");
        }
    }

    [HttpGet("days-together")]
    public async Task<ActionResult> GetDaysTogether()
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.GetCurrentRelationshipAsync(userId);
            if (relationship == null)
            {
                return RouteMessages.BadRequest("Nenhum relacionamento ativo encontrado", "Relacionamento não encontrado");
            }
            var data = new Dictionary<string, object> { { "daysTogether", relationship.DaysTogether } };
            return RouteMessages.Ok("Dias juntos calculados com sucesso", "Dias juntos", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao calcular dias juntos");
            return RouteMessages.InternalError("Erro ao calcular dias juntos", "Erro interno");
        }
    }

    [HttpPut("configuration")]
    public async Task<ActionResult> UpdateConfiguration([FromBody] UpdateRelationshipConfigurationDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.UpdateRelationshipConfigurationAsync(userId, dto);
            var data = new Dictionary<string, object> { { "relationship", relationship } };
            return RouteMessages.Ok("Configuração atualizada com sucesso", "Configuração atualizada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Dados inválidos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar configuração do relacionamento");
            return RouteMessages.InternalError("Erro ao atualizar configuração", "Erro interno");
        }
    }

    [HttpPost("unlink")]
    public async Task<ActionResult> RequestUnlink()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _relationshipService.RequestUnlinkAsync(userId);
            return RouteMessages.Ok("Solicitação de desvinculação enviada", "Solicitação enviada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar desvinculação");
            return RouteMessages.InternalError("Erro ao solicitar desvinculação", "Erro interno");
        }
    }

    [HttpPost("unlink/confirm")]
    public async Task<ActionResult> ConfirmUnlink()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _relationshipService.ConfirmUnlinkAsync(userId);
            return RouteMessages.Ok("Desvinculação confirmada", "Desvinculação realizada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Solicitação não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar desvinculação");
            return RouteMessages.InternalError("Erro ao confirmar desvinculação", "Erro interno");
        }
    }

    [HttpGet("partner")]
    public async Task<ActionResult> GetPartner()
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.GetCurrentRelationshipAsync(userId);
            
            if (relationship == null)
            {
                return RouteMessages.BadRequest("Nenhum relacionamento ativo encontrado", "Relacionamento não encontrado");
            }

            // Obter informações do parceiro
            var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
            var partnerName = relationship.User1Id == userId ? relationship.User2Name : relationship.User1Name;
            var partnerInfo = new Dictionary<string, object>
            {
                { "id", partnerId },
                { "name", partnerName }
            };

            var data = new Dictionary<string, object> { { "partner", partnerInfo } };
            return RouteMessages.Ok("Informações do parceiro obtidas com sucesso", "Parceiro", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações do parceiro");
            return RouteMessages.InternalError("Erro ao obter informações do parceiro", "Erro interno");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetRelationshipStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            var relationship = await _relationshipService.GetCurrentRelationshipAsync(userId);
            
            if (relationship == null)
            {
                return RouteMessages.BadRequest("Nenhum relacionamento ativo encontrado", "Relacionamento não encontrado");
            }

            var stats = new Dictionary<string, object>
            {
                { "daysTogether", relationship.DaysTogether },
                { "relationshipStartDate", relationship.RelationshipStartDate?.ToString("yyyy-MM-dd") ?? "" },
                { "metDate", relationship.MetDate?.ToString("yyyy-MM-dd") ?? "" },
                { "metLocation", relationship.MetLocation ?? "" },
                { "celebrationType", relationship.CelebrationType.ToString() }
            };

            var data = new Dictionary<string, object> { { "stats", stats } };
            return RouteMessages.Ok("Estatísticas do relacionamento obtidas com sucesso", "Estatísticas", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas do relacionamento");
            return RouteMessages.InternalError("Erro ao obter estatísticas", "Erro interno");
        }
    }

}



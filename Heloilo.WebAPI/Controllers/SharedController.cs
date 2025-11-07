using Heloilo.Application.DTOs.Shared;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SharedController : BaseController
{
    private readonly ISharedContentService _sharedContentService;
    private readonly ILogger<SharedController> _logger;

    public SharedController(ISharedContentService sharedContentService, ILogger<SharedController> logger)
    {
        _sharedContentService = sharedContentService;
        _logger = logger;
    }

    /// <summary>
    /// Gera link de compartilhamento para uma memória
    /// </summary>
    [HttpPost("memories/{id}")]
    [Authorize]
    public async Task<ActionResult> ShareMemory(long id, [FromQuery] int? expirationDays = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sharedContent = await _sharedContentService.CreateShareLinkAsync(userId, ContentType.Memory, id, expirationDays);
            var data = new Dictionary<string, object> { { "sharedContent", sharedContent } };
            return RouteMessages.Ok("Link de compartilhamento criado com sucesso", "Compartilhamento criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar link de compartilhamento");
            return RouteMessages.InternalError("Erro ao criar link de compartilhamento", "Erro interno");
        }
    }

    /// <summary>
    /// Gera link de compartilhamento para um desejo
    /// </summary>
    [HttpPost("wishes/{id}")]
    [Authorize]
    public async Task<ActionResult> ShareWish(long id, [FromQuery] int? expirationDays = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sharedContent = await _sharedContentService.CreateShareLinkAsync(userId, ContentType.Wish, id, expirationDays);
            var data = new Dictionary<string, object> { { "sharedContent", sharedContent } };
            return RouteMessages.Ok("Link de compartilhamento criado com sucesso", "Compartilhamento criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar link de compartilhamento");
            return RouteMessages.InternalError("Erro ao criar link de compartilhamento", "Erro interno");
        }
    }

    /// <summary>
    /// Gera link de compartilhamento para uma página de história
    /// </summary>
    [HttpPost("storypages/{id}")]
    [Authorize]
    public async Task<ActionResult> ShareStoryPage(long id, [FromQuery] int? expirationDays = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var sharedContent = await _sharedContentService.CreateShareLinkAsync(userId, ContentType.StoryPage, id, expirationDays);
            var data = new Dictionary<string, object> { { "sharedContent", sharedContent } };
            return RouteMessages.Ok("Link de compartilhamento criado com sucesso", "Compartilhamento criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar link de compartilhamento");
            return RouteMessages.InternalError("Erro ao criar link de compartilhamento", "Erro interno");
        }
    }

    /// <summary>
    /// Acessa conteúdo compartilhado (pode ser anônimo)
    /// </summary>
    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetSharedContent(string token)
    {
        try
        {
            var sharedContent = await _sharedContentService.GetSharedContentAsync(token);
            if (sharedContent == null)
            {
                return RouteMessages.NotFound("Link de compartilhamento inválido ou expirado", "Recurso não encontrado");
            }

            var data = new Dictionary<string, object> { { "sharedContent", sharedContent } };
            return RouteMessages.Ok("Conteúdo compartilhado obtido com sucesso", "Conteúdo compartilhado", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter conteúdo compartilhado");
            return RouteMessages.InternalError("Erro ao obter conteúdo compartilhado", "Erro interno");
        }
    }

    /// <summary>
    /// Revoga um link de compartilhamento
    /// </summary>
    [HttpDelete("{token}")]
    [Authorize]
    public async Task<ActionResult> RevokeShareLink(string token)
    {
        try
        {
            var userId = GetCurrentUserId();
            var revoked = await _sharedContentService.RevokeShareLinkAsync(userId, token);
            if (!revoked)
            {
                return RouteMessages.NotFound("Link de compartilhamento não encontrado", "Recurso não encontrado");
            }

            return RouteMessages.Ok("Link de compartilhamento revogado com sucesso", "Compartilhamento revogado");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar link de compartilhamento");
            return RouteMessages.InternalError("Erro ao revogar link de compartilhamento", "Erro interno");
        }
    }
}


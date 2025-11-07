using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MediaController : BaseController
{
    private readonly IMediaService _mediaService;
    private readonly ILogger<MediaController> _logger;

    public MediaController(IMediaService mediaService, ILogger<MediaController> logger)
    {
        _mediaService = mediaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém a foto de perfil de um usuário
    /// </summary>
    /// <param name="userId">ID do usuário</param>
    /// <returns>Imagem do perfil</returns>
    /// <response code="200">Foto obtida com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Usuário ou foto não encontrada</response>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult> GetUserPhoto(long userId)
    {
        try
        {
            var requestingUserId = GetCurrentUserId();
            var photo = await _mediaService.GetUserPhotoAsync(userId, requestingUserId);

            if (photo == null || photo.Length == 0)
            {
                return RouteMessages.NotFound("Foto não encontrada", "Recurso não encontrado");
            }

            return File(photo, "image/jpeg");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter foto de perfil do usuário {UserId}", userId);
            return RouteMessages.InternalError("Erro ao obter foto", "Erro interno");
        }
    }

    /// <summary>
    /// Obtém uma mídia de memória
    /// </summary>
    /// <param name="mediaId">ID da mídia</param>
    /// <returns>Arquivo de mídia</returns>
    /// <response code="200">Mídia obtida com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Mídia não encontrada</response>
    [HttpGet("memory/{mediaId}")]
    public async Task<ActionResult> GetMemoryMedia(long mediaId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var media = await _mediaService.GetMemoryMediaAsync(mediaId, userId);

            if (media == null || media.Length == 0)
            {
                return RouteMessages.NotFound("Mídia não encontrada", "Recurso não encontrado");
            }

            // Determinar content type baseado na extensão ou usar padrão
            return File(media, "image/jpeg");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter mídia de memória {MediaId}", mediaId);
            return RouteMessages.InternalError("Erro ao obter mídia", "Erro interno");
        }
    }

    /// <summary>
    /// Obtém uma mídia de mensagem
    /// </summary>
    /// <param name="mediaId">ID da mídia</param>
    /// <returns>Arquivo de mídia</returns>
    /// <response code="200">Mídia obtida com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Mídia não encontrada</response>
    [HttpGet("message/{mediaId}")]
    public async Task<ActionResult> GetMessageMedia(long mediaId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var media = await _mediaService.GetMessageMediaAsync(mediaId, userId);

            if (media == null || media.Length == 0)
            {
                return RouteMessages.NotFound("Mídia não encontrada", "Recurso não encontrado");
            }

            return File(media, "image/jpeg");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter mídia de mensagem {MediaId}", mediaId);
            return RouteMessages.InternalError("Erro ao obter mídia", "Erro interno");
        }
    }

    /// <summary>
    /// Obtém a imagem de uma página de história
    /// </summary>
    /// <param name="pageId">ID da página</param>
    /// <returns>Imagem da página</returns>
    /// <response code="200">Imagem obtida com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Página ou imagem não encontrada</response>
    [HttpGet("story/{pageId}")]
    public async Task<ActionResult> GetStoryPageImage(long pageId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var image = await _mediaService.GetStoryPageImageAsync(pageId, userId);

            if (image == null || image.Length == 0)
            {
                return RouteMessages.NotFound("Imagem não encontrada", "Recurso não encontrado");
            }

            return File(image, "image/jpeg");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter imagem da página de história {PageId}", pageId);
            return RouteMessages.InternalError("Erro ao obter imagem", "Erro interno");
        }
    }

    /// <summary>
    /// Obtém a imagem de um desejo
    /// </summary>
    /// <param name="wishId">ID do desejo</param>
    /// <returns>Imagem do desejo</returns>
    /// <response code="200">Imagem obtida com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="403">Acesso negado</response>
    /// <response code="404">Desejo ou imagem não encontrada</response>
    [HttpGet("wish/{wishId}")]
    public async Task<ActionResult> GetWishImage(long wishId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var image = await _mediaService.GetWishImageAsync(wishId, userId);

            if (image == null || image.Length == 0)
            {
                return RouteMessages.NotFound("Imagem não encontrada", "Recurso não encontrado");
            }

            return File(image, "image/jpeg");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Recurso não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter imagem do desejo {WishId}", wishId);
            return RouteMessages.InternalError("Erro ao obter imagem", "Erro interno");
        }
    }
}


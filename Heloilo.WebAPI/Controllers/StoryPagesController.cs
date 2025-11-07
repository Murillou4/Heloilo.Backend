using Heloilo.Application.DTOs.Story;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("story/[controller]")]
[Authorize]
public class StoryPagesController : BaseController
{
    private readonly IStoryService _storyService;
    private readonly IFavoriteService _favoriteService;
    private readonly ILogger<StoryPagesController> _logger;

    public StoryPagesController(IStoryService storyService, IFavoriteService favoriteService, ILogger<StoryPagesController> logger)
    {
        _storyService = storyService;
        _favoriteService = favoriteService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetStoryPages([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var pages = await _storyService.GetStoryPagesAsync(userId, startDate, endDate, sortBy, sortOrder, page, pageSize);
            return RouteMessages.OkPaged("storyPages", pages, "Páginas da história listadas com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar páginas da história");
            return RouteMessages.InternalError("Erro ao listar páginas da história", "Erro interno");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetStoryPage(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var page = await _storyService.GetStoryPageByIdAsync(id, userId);
            var data = new Dictionary<string, object> { { "storyPage", page } };
            return RouteMessages.Ok("Página da história obtida com sucesso", "Página encontrada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Página não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter página da história");
            return RouteMessages.InternalError("Erro ao obter página da história", "Erro interno");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateStoryPage([FromForm] CreateStoryPageDto dto, IFormFile? image)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var page = await _storyService.CreateStoryPageAsync(userId, dto, image);
            var data = new Dictionary<string, object> { { "storyPage", page } };
            return RouteMessages.Ok("Página da história criada com sucesso", "Página criada", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar página da história");
            return RouteMessages.InternalError("Erro ao criar página da história", "Erro interno");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateStoryPage(long id, [FromForm] CreateStoryPageDto dto, IFormFile? image)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var page = await _storyService.UpdateStoryPageAsync(id, userId, dto, image);
            var data = new Dictionary<string, object> { { "storyPage", page } };
            return RouteMessages.Ok("Página da história atualizada com sucesso", "Página atualizada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Página não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar página da história");
            return RouteMessages.InternalError("Erro ao atualizar página da história", "Erro interno");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteStoryPage(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _storyService.DeleteStoryPageAsync(id, userId);
            return RouteMessages.Ok("Página da história excluída com sucesso", "Página excluída");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Página não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir página da história");
            return RouteMessages.InternalError("Erro ao excluir página da história", "Erro interno");
        }
    }

    [HttpPost("{id}/favorite")]
    public async Task<ActionResult> AddFavorite(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _favoriteService.AddFavoriteAsync(userId, ContentType.StoryPage, id);
            return RouteMessages.Ok("Página marcada como favorita", "Favorito adicionado");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Página não encontrada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar favorito");
            return RouteMessages.InternalError("Erro ao adicionar favorito", "Erro interno");
        }
    }

    [HttpDelete("{id}/favorite")]
    public async Task<ActionResult> RemoveFavorite(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _favoriteService.RemoveFavoriteAsync(userId, ContentType.StoryPage, id);
            return RouteMessages.Ok("Favorito removido com sucesso", "Favorito removido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover favorito");
            return RouteMessages.InternalError("Erro ao remover favorito", "Erro interno");
        }
    }

    [HttpGet("favorites")]
    public async Task<ActionResult> GetFavorites([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var favorites = await _favoriteService.GetFavoritesAsync(userId, ContentType.StoryPage, page, pageSize);
            return RouteMessages.OkPaged("favorites", favorites, "Favoritos listados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar favoritos");
            return RouteMessages.InternalError("Erro ao listar favoritos", "Erro interno");
        }
    }
}

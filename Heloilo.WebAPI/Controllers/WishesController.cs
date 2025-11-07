using Heloilo.Application.DTOs.Wish;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class WishesController(IWishService wishService, ILogger<WishesController> logger) : BaseController
{
    private readonly IWishService _wishService = wishService;
    private readonly ILogger<WishesController> _logger = logger;

    /// <summary>
    /// Lista os desejos do casal com filtros e paginação
    /// </summary>
    /// <param name="categoryId">ID da categoria para filtrar (opcional)</param>
    /// <param name="search">Termo de busca para filtrar por título (opcional)</param>
    /// <param name="sortBy">Campo para ordenação: 'createdAt', 'importanceLevel' (opcional)</param>
    /// <param name="sortOrder">Ordem: 'asc' ou 'desc' (opcional)</param>
    /// <param name="page">Número da página (default: 1)</param>
    /// <param name="pageSize">Tamanho da página (default: 20, máximo: 100)</param>
    /// <returns>Lista paginada de desejos</returns>
    /// <response code="200">Desejos listados com sucesso</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpGet]
    public async Task<ActionResult> GetWishes([FromQuery] long? categoryId, [FromQuery] string? search, [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var wishes = await _wishService.GetWishesAsync(userId, categoryId, search, sortBy, sortOrder, page, pageSize);
            return RouteMessages.OkPaged("wishes", wishes, "Desejos listados com sucesso");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar desejos");
            return RouteMessages.InternalError("Erro ao listar desejos", "Erro interno");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetWish(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var wish = await _wishService.GetWishByIdAsync(id, userId);
            var data = new Dictionary<string, object> { { "wish", wish } };
            return RouteMessages.Ok("Desejo obtido com sucesso", "Desejo encontrado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Desejo não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter desejo");
            return RouteMessages.InternalError("Erro ao obter desejo", "Erro interno");
        }
    }

    /// <summary>
    /// Cria um novo desejo
    /// </summary>
    /// <param name="dto">Dados do desejo (título, descrição, link, categoria, importância)</param>
    /// <param name="image">Imagem ilustrativa do desejo (opcional, máximo 10MB)</param>
    /// <returns>Desejo criado</returns>
    /// <response code="200">Desejo criado com sucesso</response>
    /// <response code="400">Dados inválidos ou relacionamento não encontrado</response>
    /// <response code="401">Não autenticado</response>
    /// <response code="500">Erro interno do servidor</response>
    [HttpPost]
    public async Task<ActionResult> CreateWish([FromForm] CreateWishDto dto, IFormFile? image)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var wish = await _wishService.CreateWishAsync(userId, dto, image);
            var data = new Dictionary<string, object> { { "wish", wish } };
            return RouteMessages.Ok("Desejo criado com sucesso", "Desejo criado", data);
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
            _logger.LogError(ex, "Erro ao criar desejo");
            return RouteMessages.InternalError("Erro ao criar desejo", "Erro interno");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateWish(long id, [FromForm] UpdateWishDto dto, IFormFile? image)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var wish = await _wishService.UpdateWishAsync(id, userId, dto, image);
            var data = new Dictionary<string, object> { { "wish", wish } };
            return RouteMessages.Ok("Desejo atualizado com sucesso", "Desejo atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Desejo não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Dados inválidos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar desejo");
            return RouteMessages.InternalError("Erro ao atualizar desejo", "Erro interno");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWish(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _wishService.DeleteWishAsync(id, userId);
            return RouteMessages.Ok("Desejo excluído com sucesso", "Desejo excluído");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Desejo não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir desejo");
            return RouteMessages.InternalError("Erro ao excluir desejo", "Erro interno");
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        try
        {
            var categories = await _wishService.GetCategoriesAsync();
            var data = new Dictionary<string, object> { { "categories", categories } };
            return RouteMessages.Ok("Categorias listadas com sucesso", "Categorias", data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar categorias");
            return RouteMessages.InternalError("Erro ao listar categorias", "Erro interno");
        }
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult> GetComments(long id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var comments = await _wishService.GetWishCommentsAsync(id, page, pageSize);
            return RouteMessages.OkPaged("comments", comments, "Comentários listados com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar comentários");
            return RouteMessages.InternalError("Erro ao listar comentários", "Erro interno");
        }
    }

    [HttpPost("{id}/comments")]
    public async Task<ActionResult> AddComment(long id, [FromBody] CreateWishCommentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var comment = await _wishService.AddCommentAsync(id, userId, dto);
            var data = new Dictionary<string, object> { { "comment", comment } };
            return RouteMessages.Ok("Comentário adicionado com sucesso", "Comentário criado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Desejo não encontrado");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Dados inválidos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar comentário");
            return RouteMessages.InternalError("Erro ao adicionar comentário", "Erro interno");
        }
    }

    [HttpPut("comments/{id}")]
    public async Task<ActionResult> UpdateComment(long id, [FromBody] CreateWishCommentDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var comment = await _wishService.UpdateCommentAsync(id, userId, dto);
            var data = new Dictionary<string, object> { { "comment", comment } };
            return RouteMessages.Ok("Comentário atualizado com sucesso", "Comentário atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Comentário não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Dados inválidos");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar comentário");
            return RouteMessages.InternalError("Erro ao atualizar comentário", "Erro interno");
        }
    }

    [HttpDelete("comments/{id}")]
    public async Task<ActionResult> DeleteComment(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _wishService.DeleteCommentAsync(id, userId);
            return RouteMessages.Ok("Comentário excluído com sucesso", "Comentário excluído");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Comentário não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir comentário");
            return RouteMessages.InternalError("Erro ao excluir comentário", "Erro interno");
        }
    }

    [HttpGet("stats")]
    public async Task<ActionResult> GetWishStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            var wishes = await _wishService.GetWishesAsync(userId, null, null, null, null, 1, 1000);
            
            var stats = new Dictionary<string, object>
            {
                { "totalWishes", wishes.TotalItems },
                { "byCategory", wishes.Items.GroupBy(w => w.CategoryName ?? "Sem categoria")
                    .ToDictionary(g => g.Key, g => g.Count()) },
                { "byUser", wishes.Items.GroupBy(w => w.UserId)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()) },
                { "byImportance", wishes.Items.GroupBy(w => w.ImportanceLevel)
                    .OrderByDescending(g => g.Key)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()) }
            };
            
            var data = new Dictionary<string, object> { { "stats", stats } };
            return RouteMessages.Ok("Estatísticas obtidas com sucesso", "Estatísticas", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de desejos");
            return RouteMessages.InternalError("Erro ao obter estatísticas", "Erro interno");
        }
    }

    [HttpPost("{id}/mark-important")]
    public async Task<ActionResult> MarkAsImportant(long id, [FromBody] int? importanceLevel = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var wish = await _wishService.GetWishByIdAsync(id, userId);
            
            var updateDto = new Heloilo.Application.DTOs.Wish.UpdateWishDto
            {
                ImportanceLevel = importanceLevel ?? 5 // Máxima importância se não especificado
            };
            
            var updatedWish = await _wishService.UpdateWishAsync(id, userId, updateDto);
            var data = new Dictionary<string, object> { { "wish", updatedWish } };
            return RouteMessages.Ok("Desejo marcado como importante", "Desejo atualizado", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Desejo não encontrado");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar desejo como importante");
            return RouteMessages.InternalError("Erro ao atualizar desejo", "Erro interno");
        }
    }

}


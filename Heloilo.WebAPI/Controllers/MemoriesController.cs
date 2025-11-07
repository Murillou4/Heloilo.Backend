using Heloilo.Application.DTOs.Memory;
using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class MemoriesController : BaseController
{
    private readonly IMemoryService _memoryService;
    private readonly ILogger<MemoriesController> _logger;

    public MemoriesController(IMemoryService memoryService, ILogger<MemoriesController> logger)
    {
        _memoryService = memoryService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetMemories([FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] List<string>? tags, [FromQuery] string? search, [FromQuery] string? sortBy, [FromQuery] string? sortOrder, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var memories = await _memoryService.GetMemoriesAsync(userId, startDate, endDate, tags, search, sortBy, sortOrder, page, pageSize);
            return RouteMessages.OkPaged("memories", memories, "Memórias listadas com sucesso");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar memórias");
            return RouteMessages.InternalError("Erro ao listar memórias", "Erro interno");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetMemory(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var memory = await _memoryService.GetMemoryByIdAsync(id, userId);
            var data = new Dictionary<string, object> { { "memory", memory } };
            return RouteMessages.Ok("Memória obtida com sucesso", "Memória encontrada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Memória não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter memória");
            return RouteMessages.InternalError("Erro ao obter memória", "Erro interno");
        }
    }

    [HttpPost]
    public async Task<ActionResult> CreateMemory([FromForm] CreateMemoryDto dto, List<IFormFile>? media)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var memory = await _memoryService.CreateMemoryAsync(userId, dto, media);
            var data = new Dictionary<string, object> { { "memory", memory } };
            return RouteMessages.Ok("Memória criada com sucesso", "Memória criada", data);
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
            _logger.LogError(ex, "Erro ao criar memória");
            return RouteMessages.InternalError("Erro ao criar memória", "Erro interno");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateMemory(long id, [FromBody] CreateMemoryDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var memory = await _memoryService.UpdateMemoryAsync(id, userId, dto);
            var data = new Dictionary<string, object> { { "memory", memory } };
            return RouteMessages.Ok("Memória atualizada com sucesso", "Memória atualizada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Memória não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar memória");
            return RouteMessages.InternalError("Erro ao atualizar memória", "Erro interno");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMemory(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _memoryService.DeleteMemoryAsync(id, userId);
            return RouteMessages.Ok("Memória excluída com sucesso", "Memória excluída");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Memória não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir memória");
            return RouteMessages.InternalError("Erro ao excluir memória", "Erro interno");
        }
    }

    [HttpPost("{id}/media")]
    public async Task<ActionResult> AddMedia(long id, IFormFile file)
    {
        try
        {
            var userId = GetCurrentUserId();
            var mediaId = await _memoryService.AddMediaAsync(id, userId, file);
            var data = new Dictionary<string, object> { { "mediaId", mediaId } };
            return RouteMessages.Ok("Mídia adicionada com sucesso", "Mídia adicionada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Memória não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Arquivo inválido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar mídia");
            return RouteMessages.InternalError("Erro ao adicionar mídia", "Erro interno");
        }
    }

    [HttpDelete("{id}/media/{mediaId}")]
    public async Task<ActionResult> DeleteMedia(long id, long mediaId)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _memoryService.DeleteMediaAsync(id, mediaId, userId);
            return RouteMessages.Ok("Mídia excluída com sucesso", "Mídia excluída");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Mídia não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir mídia");
            return RouteMessages.InternalError("Erro ao excluir mídia", "Erro interno");
        }
    }

    [HttpGet("tags")]
    public async Task<ActionResult> GetTags()
    {
        try
        {
            var userId = GetCurrentUserId();
            var tags = await _memoryService.GetTagsAsync(userId);
            var data = new Dictionary<string, object> { { "tags", tags } };
            return RouteMessages.Ok("Tags listadas com sucesso", "Tags", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar tags");
            return RouteMessages.InternalError("Erro ao listar tags", "Erro interno");
        }
    }

    [HttpGet("{id}/media")]
    public async Task<ActionResult> GetMemoryMedia(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var memory = await _memoryService.GetMemoryByIdAsync(id, userId);
            var data = new Dictionary<string, object> 
            { 
                { "mediaCount", memory.MediaCount },
                { "tags", memory.Tags ?? new List<string>() }
            };
            return RouteMessages.Ok("Informações da memória obtidas com sucesso", "Memória", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Memória não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar mídias da memória");
            return RouteMessages.InternalError("Erro ao listar mídias", "Erro interno");
        }
    }

}

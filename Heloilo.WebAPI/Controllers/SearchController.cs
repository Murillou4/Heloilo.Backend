using Heloilo.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class SearchController : BaseController
{
    private readonly IMemoryService _memoryService;
    private readonly IWishService _wishService;
    private readonly IChatService _chatService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        IMemoryService memoryService,
        IWishService wishService,
        IChatService chatService,
        ILogger<SearchController> logger)
    {
        _memoryService = memoryService;
        _wishService = wishService;
        _chatService = chatService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Search([FromQuery] string query, [FromQuery] string? type = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RouteMessages.BadRequest("Termo de busca não informado", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var results = new Dictionary<string, object>();

            type = type?.ToLower();
            var searchAll = string.IsNullOrEmpty(type) || type == "all";

            if (searchAll || type == "memories")
            {
                var memories = await _memoryService.GetMemoriesAsync(userId, null, null, null, query, null, null, page, pageSize);
                results["memories"] = new
                {
                    items = memories.Items,
                    totalItems = memories.TotalItems,
                    page = memories.Page,
                    pageSize = memories.PageSize,
                    totalPages = memories.TotalPages
                };
            }

            if (searchAll || type == "wishes")
            {
                var wishes = await _wishService.GetWishesAsync(userId, null, query, null, null, null, page, pageSize);
                results["wishes"] = new
                {
                    items = wishes.Items,
                    totalItems = wishes.TotalItems,
                    page = wishes.Page,
                    pageSize = wishes.PageSize,
                    totalPages = wishes.TotalPages
                };
            }

            if (searchAll || type == "messages")
            {
                var messages = await _chatService.SearchMessagesAsync(userId, query);
                results["messages"] = messages;
            }

            var data = new Dictionary<string, object>
            {
                { "results", results },
                { "query", query },
                { "type", type ?? "all" }
            };

            return RouteMessages.Ok("Busca realizada com sucesso", "Resultados encontrados", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar busca");
            return RouteMessages.InternalError("Erro ao realizar busca", "Erro interno");
        }
    }
}







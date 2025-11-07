using Heloilo.Application.DTOs.Chat;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using API;

namespace Heloilo.WebAPI.Controllers;

[ApiController]
[Route("chat/[controller]")]
[Authorize]
public class ChatMessagesController : BaseController
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatMessagesController> _logger;

    public ChatMessagesController(IChatService chatService, ILogger<ChatMessagesController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetMessages([FromQuery] MessageType? messageType, [FromQuery] DeliveryStatus? deliveryStatus, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetCurrentUserId();
            var messages = await _chatService.GetMessagesAsync(userId, messageType, deliveryStatus, startDate, endDate, search, page, pageSize);
            return RouteMessages.OkPaged("messages", messages, "Mensagens listadas com sucesso");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar mensagens");
            return RouteMessages.InternalError("Erro ao listar mensagens", "Erro interno");
        }
    }

    [HttpPost]
    public async Task<ActionResult> SendMessage([FromForm] SendMessageDto dto, IFormFile? media)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return RouteMessages.BadRequest("Dados inválidos", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var message = await _chatService.SendMessageAsync(userId, dto, media);
            var data = new Dictionary<string, object> { { "message", message } };
            return RouteMessages.Ok("Mensagem enviada com sucesso", "Mensagem enviada", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (ArgumentException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Arquivo inválido");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar mensagem");
            return RouteMessages.InternalError("Erro ao enviar mensagem", "Erro interno");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult> SearchMessages([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RouteMessages.BadRequest("Termo de busca não informado", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            var messages = await _chatService.SearchMessagesAsync(userId, searchTerm);
            var data = new Dictionary<string, object> { { "messages", messages } };
            return RouteMessages.Ok("Busca realizada com sucesso", "Mensagens encontradas", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao pesquisar mensagens");
            return RouteMessages.InternalError("Erro ao pesquisar mensagens", "Erro interno");
        }
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult> MarkAsRead(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _chatService.MarkAsReadAsync(id, userId);
            return RouteMessages.Ok("Mensagem marcada como lida", "Mensagem atualizada");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Mensagem não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar mensagem como lida");
            return RouteMessages.InternalError("Erro ao marcar mensagem como lida", "Erro interno");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _chatService.DeleteMessageAsync(id, userId);
            return RouteMessages.Ok("Mensagem excluída com sucesso", "Mensagem excluída");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Mensagem não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir mensagem");
            return RouteMessages.InternalError("Erro ao excluir mensagem", "Erro interno");
        }
    }

    [HttpPost("mark-read-multiple")]
    public async Task<ActionResult> MarkMultipleAsRead([FromBody] List<long> messageIds)
    {
        try
        {
            if (messageIds == null || !messageIds.Any())
            {
                return RouteMessages.BadRequest("Lista de IDs de mensagens não pode estar vazia", "Erro de validação");
            }

            var userId = GetCurrentUserId();
            await _chatService.MarkMultipleAsReadAsync(userId, messageIds);
            return RouteMessages.Ok("Mensagens marcadas como lidas com sucesso", "Mensagens atualizadas");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar múltiplas mensagens como lidas");
            return RouteMessages.InternalError("Erro ao marcar mensagens como lidas", "Erro interno");
        }
    }

    [HttpPost("typing")]
    public async Task<ActionResult> SetTypingIndicator([FromBody] bool isTyping)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _chatService.SetTypingIndicatorAsync(userId, isTyping);
            return RouteMessages.Ok("Indicador de digitação atualizado", "Status atualizado");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir indicador de digitação");
            return RouteMessages.InternalError("Erro ao definir indicador de digitação", "Erro interno");
        }
    }

    [HttpGet("typing")]
    public async Task<ActionResult> GetPartnerTypingStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            var isTyping = await _chatService.GetPartnerTypingStatusAsync(userId);
            var data = new Dictionary<string, object> { { "isTyping", isTyping } };
            return RouteMessages.Ok("Status de digitação obtido com sucesso", "Status", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter status de digitação");
            return RouteMessages.InternalError("Erro ao obter status de digitação", "Erro interno");
        }
    }

    [HttpGet("unread")]
    public async Task<ActionResult> GetUnreadCount()
    {
        try
        {
            var userId = GetCurrentUserId();
            var messages = await _chatService.GetMessagesAsync(userId, null, DeliveryStatus.Sent, null, null, null, 1, 1);
            var data = new Dictionary<string, object> { { "count", messages.TotalItems } };
            return RouteMessages.Ok("Contagem de mensagens não lidas obtida com sucesso", "Contagem", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem de mensagens não lidas");
            return RouteMessages.InternalError("Erro ao obter contagem", "Erro interno");
        }
    }

    [HttpPost("{id}/deliver")]
    public async Task<ActionResult> MarkAsDelivered(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            // Marcar como entregue é feito automaticamente quando marca como lida
            // Este endpoint força o status de entregue
            await _chatService.MarkAsReadAsync(id, userId);
            return RouteMessages.Ok("Mensagem marcada como entregue", "Status atualizado");
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.NotFound(ex.Message, "Mensagem não encontrada");
        }
        catch (UnauthorizedAccessException ex)
        {
            return RouteMessages.Unauthorized(ex.Message, "Acesso negado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao marcar mensagem como entregue");
            return RouteMessages.InternalError("Erro ao atualizar status", "Erro interno");
        }
    }

    [HttpGet("latest")]
    public async Task<ActionResult> GetLatestMessages([FromQuery] int limit = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var messages = await _chatService.GetMessagesAsync(userId, null, null, null, null, null, 1, limit);
            var data = new Dictionary<string, object> { { "messages", messages.Items } };
            return RouteMessages.Ok("Últimas mensagens obtidas com sucesso", "Mensagens", data);
        }
        catch (KeyNotFoundException ex)
        {
            return RouteMessages.BadRequest(ex.Message, "Relacionamento não encontrado");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter últimas mensagens");
            return RouteMessages.InternalError("Erro ao obter mensagens", "Erro interno");
        }
    }

}

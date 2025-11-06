using Heloilo.Application.DTOs.Chat;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Heloilo.Application.Hubs;

namespace Heloilo.Application.Services;

public class ChatService : IChatService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<ChatService> _logger;
    private readonly IMemoryCache _cache;
    private readonly IHubContext<ChatHub> _hubContext;
    private const int TYPING_INDICATOR_TIMEOUT_SECONDS = 5;

    public ChatService(HeloiloDbContext context, ILogger<ChatService> logger, IMemoryCache cache, IHubContext<ChatHub> hubContext)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
        _hubContext = hubContext;
    }

    public async Task<PagedResult<ChatMessageDto>> GetMessagesAsync(long userId, MessageType? messageType = null, DeliveryStatus? deliveryStatus = null, DateOnly? startDate = null, DateOnly? endDate = null, string? search = null, int page = 1, int pageSize = 50)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 50, maxPageSize: 100);

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Media)
            .Where(m => m.RelationshipId == relationship.Id && m.DeletedAt == null);

        if (messageType.HasValue)
        {
            query = query.Where(m => m.MessageType == messageType.Value);
        }

        if (deliveryStatus.HasValue)
        {
            query = query.Where(m => m.DeliveryStatus == deliveryStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(m => m.Content != null && m.Content.Contains(search));
        }

        if (startDate.HasValue)
        {
            query = query.Where(m => DateOnly.FromDateTime(m.SentAt) >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(m => DateOnly.FromDateTime(m.SentAt) <= endDate.Value);
        }

        var totalItems = await query.CountAsync();

        var messages = await query
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<ChatMessageDto>
        {
            Items = messages.OrderBy(m => m.SentAt).Select(MapToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<bool> DeleteMessageAsync(long messageId, long userId)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.DeletedAt == null);

        if (message == null) throw new KeyNotFoundException("Mensagem não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || message.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        if (message.SenderId != userId)
            throw new UnauthorizedAccessException("Você só pode excluir suas próprias mensagens");

        message.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ChatMessageDto> SendMessageAsync(long userId, SendMessageDto dto, IFormFile? media = null)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var message = new ChatMessage
        {
            RelationshipId = relationship.Id,
            SenderId = userId,
            Content = dto.Content,
            MessageType = dto.MessageType,
            DeliveryStatus = DeliveryStatus.Sent,
            SentAt = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // Adicionar mídia se fornecida
        if (media != null)
        {
            var (isValid, errorMessage) = dto.MessageType == MessageType.Audio
                ? ValidationHelper.ValidateAudioFile(media)
                : ValidationHelper.ValidateImageFile(media);
            
            if (!isValid)
            {
                throw new ArgumentException(errorMessage);
            }

            using var stream = new MemoryStream();
            await media.CopyToAsync(stream);

            var messageMedia = new MessageMedia
            {
                ChatMessageId = message.Id,
                FileBlob = stream.ToArray(),
                FileType = Path.GetExtension(media.FileName),
                FileSize = media.Length,
                MimeType = media.ContentType
            };

        _context.MessageMedia.Add(messageMedia);
        await _context.SaveChangesAsync();
    }

    var messageDto = await GetMessageByIdAsync(message.Id);

    // Enviar mensagem via SignalR
    try
    {
        await _hubContext.Clients.Group($"relationship:{relationship.Id}")
            .SendAsync("NewMessage", messageDto);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Erro ao enviar mensagem via SignalR");
    }

    return messageDto;
}

    public async Task<List<ChatMessageDto>> SearchMessagesAsync(long userId, string searchTerm)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var messages = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Media)
            .Where(m => m.RelationshipId == relationship.Id && 
                       m.DeletedAt == null &&
                       (m.Content != null && m.Content.Contains(searchTerm)))
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .ToListAsync();

        return messages.Select(MapToDto).ToList();
    }

    public async Task<bool> MarkAsReadAsync(long messageId, long userId)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.DeletedAt == null);

        if (message == null) throw new KeyNotFoundException("Mensagem não encontrada");

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null || message.RelationshipId != relationship.Id)
            throw new UnauthorizedAccessException("Acesso negado");

        // Só marca como lida se não for o próprio remetente
        if (message.SenderId != userId)
        {
            if (message.DeliveryStatus == DeliveryStatus.Sent)
            {
                message.DeliveryStatus = DeliveryStatus.Delivered;
                message.DeliveredAt = DateTime.UtcNow;
            }

            if (message.DeliveryStatus == DeliveryStatus.Delivered)
            {
                message.DeliveryStatus = DeliveryStatus.Read;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Notificar via SignalR sobre atualização de status
            try
            {
                await _hubContext.Clients.Group($"relationship:{relationship.Id}")
                    .SendAsync("DeliveryStatusUpdated", new { messageId = message.Id, status = message.DeliveryStatus.ToString() });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao notificar atualização de status via SignalR");
            }
        }

        return true;
    }

    public async Task<bool> MarkMultipleAsReadAsync(long userId, List<long> messageIds)
    {
        if (messageIds == null || !messageIds.Any())
        {
            return false;
        }

        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var messages = await _context.ChatMessages
            .Where(m => messageIds.Contains(m.Id) && 
                       m.RelationshipId == relationship.Id && 
                       m.DeletedAt == null &&
                       m.SenderId != userId)
            .ToListAsync();

        foreach (var message in messages)
        {
            if (message.DeliveryStatus == DeliveryStatus.Sent)
            {
                message.DeliveryStatus = DeliveryStatus.Delivered;
                message.DeliveredAt = DateTime.UtcNow;
            }

            if (message.DeliveryStatus == DeliveryStatus.Delivered)
            {
                message.DeliveryStatus = DeliveryStatus.Read;
                message.ReadAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SetTypingIndicatorAsync(long userId, bool isTyping)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
        var cacheKey = $"typing:{relationship.Id}:{partnerId}";

        if (isTyping)
        {
            _cache.Set(cacheKey, true, TimeSpan.FromSeconds(TYPING_INDICATOR_TIMEOUT_SECONDS));
        }
        else
        {
            _cache.Remove(cacheKey);
        }

        await Task.CompletedTask;
    }

    public async Task<bool> GetPartnerTypingStatusAsync(long userId)
    {
        var relationship = await GetRelationshipAsync(userId);
        if (relationship == null) throw new KeyNotFoundException("Relacionamento não encontrado");

        // O parceiro está digitando para este usuário, então a chave deve ser baseada no partnerId
        var partnerId = relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
        var cacheKey = $"typing:{relationship.Id}:{partnerId}";

        return _cache.TryGetValue(cacheKey, out _);
    }

    private async Task<ChatMessageDto> GetMessageByIdAsync(long id)
    {
        var message = await _context.ChatMessages
            .Include(m => m.Sender)
            .Include(m => m.Media)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null) throw new KeyNotFoundException("Mensagem não encontrada");

        return MapToDto(message);
    }

    private async Task<Relationship?> GetRelationshipAsync(long userId)
    {
        return await _context.Relationships
            .FirstOrDefaultAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);
    }

    private static ChatMessageDto MapToDto(ChatMessage message)
    {
        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderName = message.Sender.Name,
            Content = message.Content,
            MessageType = message.MessageType,
            DeliveryStatus = message.DeliveryStatus,
            SentAt = message.SentAt,
            DeliveredAt = message.DeliveredAt,
            ReadAt = message.ReadAt,
            HasMedia = message.Media != null && message.Media.Any()
        };
    }
}


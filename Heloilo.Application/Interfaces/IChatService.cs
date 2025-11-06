using Heloilo.Application.DTOs.Chat;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Interfaces;

public interface IChatService
{
    Task<PagedResult<ChatMessageDto>> GetMessagesAsync(long userId, MessageType? messageType = null, DeliveryStatus? deliveryStatus = null, DateOnly? startDate = null, DateOnly? endDate = null, string? search = null, int page = 1, int pageSize = 50);
    Task<bool> DeleteMessageAsync(long messageId, long userId);
    Task<ChatMessageDto> SendMessageAsync(long userId, SendMessageDto dto, IFormFile? media = null);
    Task<List<ChatMessageDto>> SearchMessagesAsync(long userId, string searchTerm);
    Task<bool> MarkAsReadAsync(long messageId, long userId);
    Task<bool> MarkMultipleAsReadAsync(long userId, List<long> messageIds);
    Task SetTypingIndicatorAsync(long userId, bool isTyping);
    Task<bool> GetPartnerTypingStatusAsync(long userId);
}


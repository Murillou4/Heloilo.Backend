using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Chat;

public class ChatMessageDto
{
    public long Id { get; set; }
    public long SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? Content { get; set; }
    public MessageType MessageType { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool HasMedia { get; set; }
}


using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Relationship;

public class RelationshipInvitationDto
{
    public long Id { get; set; }
    public long SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string? SenderNickname { get; set; }
    public long ReceiverId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    public string? ReceiverNickname { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}


using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Shared;

public class SharedContentDto
{
    public long Id { get; set; }
    public ContentType ContentType { get; set; }
    public long ContentId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string ShareUrl { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public int AccessCount { get; set; }
    public object? Content { get; set; } // O conteúdo compartilhado
}


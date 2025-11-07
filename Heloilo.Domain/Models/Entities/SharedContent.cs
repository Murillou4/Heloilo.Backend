using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class SharedContent : BaseEntity
{
    public long RelationshipId { get; set; }
    public ContentType ContentType { get; set; }
    public long ContentId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedAt { get; set; }
    public int AccessCount { get; set; } = 0;
    public DateTime? LastAccessedAt { get; set; }

    // Navigation property
    public virtual Relationship Relationship { get; set; } = null!;
}


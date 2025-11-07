using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class EmailVerificationToken : BaseEntity
{
    public long UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public DateTime? UsedAt { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}


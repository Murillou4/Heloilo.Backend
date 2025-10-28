using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class UserStatus : BaseEntity
{
    public long UserId { get; set; }

    public string CurrentStatus { get; set; } = string.Empty;

    public DateTime StatusUpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}

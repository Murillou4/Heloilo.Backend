using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class WishComment : BaseEntity, ISoftDeletable
{
    public long WishId { get; set; }
    public long UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual Wish Wish { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

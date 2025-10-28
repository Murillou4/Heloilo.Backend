using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class WishCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Emoji { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Wish> Wishes { get; set; } = new List<Wish>();
}

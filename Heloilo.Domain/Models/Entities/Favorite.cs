using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class Favorite : BaseEntity
{
    public long UserId { get; set; }
    public ContentType ContentType { get; set; }
    public long ContentId { get; set; }

    // Navigation property
    public virtual User User { get; set; } = null!;
}


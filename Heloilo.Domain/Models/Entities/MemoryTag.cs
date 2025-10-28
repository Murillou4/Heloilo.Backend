using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class MemoryTag : BaseEntity
{
    public long MemoryId { get; set; }

    public string TagName { get; set; } = string.Empty;

    // Navigation properties
    public virtual Memory Memory { get; set; } = null!;
}

using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class Memory : BaseEntity, ISoftDeletable
{
    public long RelationshipId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateOnly MemoryDate { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual Relationship Relationship { get; set; } = null!;
    public virtual ICollection<MemoryMedia> Media { get; set; } = new List<MemoryMedia>();
    public virtual ICollection<MemoryTag> Tags { get; set; } = new List<MemoryTag>();
}

using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class StoryPage : BaseEntity, ISoftDeletable
{
    public long RelationshipId { get; set; }

    public int PageNumber { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public byte[]? ImageBlob { get; set; }

    public DateOnly PageDate { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual Relationship Relationship { get; set; } = null!;
}

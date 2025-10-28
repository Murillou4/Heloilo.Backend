using Heloilo.Domain.Models.Common;


namespace Heloilo.Domain.Models.Entities;

public class Wish : BaseEntity, ISoftDeletable
{
    public long UserId { get; set; }
    public long RelationshipId { get; set; }
    public long? CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? LinkUrl { get; set; }

    public byte[]? ImageBlob { get; set; }

    public int ImportanceLevel { get; set; } = 3;

    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Relationship Relationship { get; set; } = null!;
    public virtual WishCategory? Category { get; set; }
    public virtual ICollection<WishComment> Comments { get; set; } = new List<WishComment>();
}

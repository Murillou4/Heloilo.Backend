using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class InitialSetup : BaseEntity
{
    public long RelationshipId { get; set; }
    public long UserId { get; set; }

    public bool IsCompleted { get; set; } = false;

    public bool IsSkipped { get; set; } = false;

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public virtual Relationship Relationship { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

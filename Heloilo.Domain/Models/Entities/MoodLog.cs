using Heloilo.Domain.Models.Common;

namespace Heloilo.Domain.Models.Entities;

public class MoodLog : BaseEntity
{
    public long UserId { get; set; }
    public long RelationshipId { get; set; }
    public long MoodTypeId { get; set; }

    public string? Comment { get; set; }

    public DateOnly LogDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Relationship Relationship { get; set; } = null!;
    public virtual MoodType MoodType { get; set; } = null!;
}

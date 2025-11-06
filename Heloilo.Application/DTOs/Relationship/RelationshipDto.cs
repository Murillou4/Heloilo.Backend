using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Relationship;

public class RelationshipDto
{
    public long Id { get; set; }
    public long User1Id { get; set; }
    public string User1Name { get; set; } = string.Empty;
    public long User2Id { get; set; }
    public string User2Name { get; set; } = string.Empty;
    public DateOnly? MetDate { get; set; }
    public string? MetLocation { get; set; }
    public DateOnly? RelationshipStartDate { get; set; }
    public CelebrationType CelebrationType { get; set; }
    public bool IsActive { get; set; }
    public int DaysTogether { get; set; }
    public DateTime CreatedAt { get; set; }
}


namespace Heloilo.Application.DTOs.Celebration;

public class AnniversaryInfoDto
{
    public int DaysTogether { get; set; }
    public DateOnly? RelationshipStartDate { get; set; }
    public DateOnly? NextAnniversary { get; set; }
    public int DaysUntilNextAnniversary { get; set; }
    public bool IsAnniversaryToday { get; set; }
}


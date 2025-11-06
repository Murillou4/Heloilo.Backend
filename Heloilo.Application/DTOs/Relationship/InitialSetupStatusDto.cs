namespace Heloilo.Application.DTOs.Relationship;

public class InitialSetupStatusDto
{
    public bool IsCompleted { get; set; }
    public bool IsSkipped { get; set; }
    public bool CurrentUserCompleted { get; set; }
    public bool PartnerCompleted { get; set; }
    public bool CanAccessApp { get; set; }
}


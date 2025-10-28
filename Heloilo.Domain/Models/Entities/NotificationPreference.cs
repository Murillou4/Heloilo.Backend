using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class NotificationPreference : BaseEntity
{
    public long UserId { get; set; }

    public NotificationType NotificationType { get; set; }

    public bool IsEnabled { get; set; } = true;

    public TimeOnly? QuietStartTime { get; set; }

    public TimeOnly? QuietEndTime { get; set; }

    public IntensityLevel IntensityLevel { get; set; } = IntensityLevel.Normal;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}

using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Domain.Models.Entities;

public class MoodType : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Emoji { get; set; } = string.Empty;

    public MoodCategory MoodCategory { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<MoodLog> MoodLogs { get; set; } = new List<MoodLog>();
}

using System.ComponentModel.DataAnnotations;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Notification;

public class NotificationPreferenceDto : IValidatableObject
{
    [Required(ErrorMessage = "Tipo de notificação é obrigatório")]
    public NotificationType NotificationType { get; set; }
    
    public bool IsEnabled { get; set; } = true;
    
    public TimeOnly? QuietStartTime { get; set; }
    
    public TimeOnly? QuietEndTime { get; set; }
    
    [Required(ErrorMessage = "Nível de intensidade é obrigatório")]
    public IntensityLevel IntensityLevel { get; set; } = IntensityLevel.Normal;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (QuietStartTime.HasValue && QuietEndTime.HasValue)
        {
            if (QuietStartTime >= QuietEndTime)
            {
                yield return new ValidationResult(
                    "Horário de início do período silencioso deve ser anterior ao horário de fim",
                    new[] { nameof(QuietStartTime), nameof(QuietEndTime) });
            }
        }
    }
}


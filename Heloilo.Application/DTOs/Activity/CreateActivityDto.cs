using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Activity;

public class CreateActivityDto : IValidatableObject
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(500, ErrorMessage = "Título deve ter no máximo 500 caracteres")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Data da atividade é obrigatória")]
    public DateOnly ActivityDate { get; set; }

    [Range(5, 1440, ErrorMessage = "Lembrete deve estar entre 5 e 1440 minutos")]
    public int? ReminderMinutes { get; set; }
    
    // Validação: reminder_minutes deve ser um dos valores permitidos (5, 15, 30, 60)
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReminderMinutes.HasValue)
        {
            var allowedValues = new[] { 5, 15, 30, 60 };
            if (!allowedValues.Contains(ReminderMinutes.Value))
            {
                yield return new ValidationResult("Lembrete deve ser 5, 15, 30 ou 60 minutos", new[] { nameof(ReminderMinutes) });
            }
        }
    }
}

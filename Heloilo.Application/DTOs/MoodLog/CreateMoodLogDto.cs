using System.ComponentModel.DataAnnotations;
using Heloilo.Application.Helpers;

namespace Heloilo.Application.DTOs.MoodLog;

public class CreateMoodLogDto : IValidatableObject
{
    [Required(ErrorMessage = "Tipo de humor é obrigatório")]
    [Range(1, long.MaxValue, ErrorMessage = "Tipo de humor inválido")]
    public long MoodTypeId { get; set; }

    [MaxLength(2000, ErrorMessage = "Comentário deve ter no máximo 2000 caracteres")]
    public string? Comment { get; set; }

    public DateOnly? LogDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (LogDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (LogDate.Value > today)
            {
                yield return new ValidationResult("Data do registro não pode ser futura", new[] { nameof(LogDate) });
            }
            if (LogDate.Value.Year < 1900)
            {
                yield return new ValidationResult("Data do registro não pode ser anterior a 1900", new[] { nameof(LogDate) });
            }
            
            // Validar existência da data
            if (!ValidationHelper.IsValidDate(LogDate.Value.Day, LogDate.Value.Month, LogDate.Value.Year))
            {
                yield return new ValidationResult("Data inválida", new[] { nameof(LogDate) });
            }
        }
    }
}


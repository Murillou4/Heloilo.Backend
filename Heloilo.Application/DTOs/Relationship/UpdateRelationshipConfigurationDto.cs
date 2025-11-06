using System.ComponentModel.DataAnnotations;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Relationship;

public class UpdateRelationshipConfigurationDto : IValidatableObject
{
    public DateOnly? MetDate { get; set; }

    [MaxLength(255, ErrorMessage = "Local deve ter no máximo 255 caracteres")]
    public string? MetLocation { get; set; }

    public DateOnly? RelationshipStartDate { get; set; }

    public CelebrationType? CelebrationType { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validar que a data de início não seja futura e não seja anterior a 1900
        if (RelationshipStartDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (RelationshipStartDate.Value > today)
            {
                yield return new ValidationResult("Data de início do relacionamento não pode ser futura", new[] { nameof(RelationshipStartDate) });
            }
            if (RelationshipStartDate.Value.Year < 1900)
            {
                yield return new ValidationResult("Data de início do relacionamento não pode ser anterior a 1900", new[] { nameof(RelationshipStartDate) });
            }
        }

        // Validar que a data em que se conheceram não seja futura
        if (MetDate.HasValue)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (MetDate.Value > today)
            {
                yield return new ValidationResult("Data em que se conheceram não pode ser futura", new[] { nameof(MetDate) });
            }
        }
    }
}


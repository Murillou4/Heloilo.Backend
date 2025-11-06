using System.ComponentModel.DataAnnotations;
using Heloilo.Application.Helpers;

namespace Heloilo.Application.DTOs.Story;

public class CreateStoryPageDto : IValidatableObject
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(500, ErrorMessage = "Título deve ter no máximo 500 caracteres")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000, ErrorMessage = "Conteúdo deve ter no máximo 5000 caracteres")]
    public string? Content { get; set; }

    [Required(ErrorMessage = "Data da página é obrigatória")]
    public DateOnly PageDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Validar que a data não seja futura e não seja anterior a 1900
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (PageDate > today)
        {
            yield return new ValidationResult("Data da página não pode ser futura", new[] { nameof(PageDate) });
        }
        if (PageDate.Year < 1900)
        {
            yield return new ValidationResult("Data da página não pode ser anterior a 1900", new[] { nameof(PageDate) });
        }
        
        // Validar existência da data
        if (!ValidationHelper.IsValidDate(PageDate.Day, PageDate.Month, PageDate.Year))
        {
            yield return new ValidationResult("Data inválida", new[] { nameof(PageDate) });
        }
    }
}


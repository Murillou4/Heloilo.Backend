using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Memory;

public class CreateMemoryDto : IValidatableObject
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(500, ErrorMessage = "Título deve ter no máximo 500 caracteres")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Data da memória é obrigatória")]
    public DateOnly MemoryDate { get; set; }

    public List<string>? Tags { get; set; }
    
    // Validação: tags não podem exceder 50 caracteres cada e data não pode ser futura
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();
        
        // Validar que a data não seja futura e não seja anterior a 1900
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (MemoryDate > today)
        {
            results.Add(new ValidationResult("Data da memória não pode ser futura", new[] { nameof(MemoryDate) }));
        }
        if (MemoryDate.Year < 1900)
        {
            results.Add(new ValidationResult("Data da memória não pode ser anterior a 1900", new[] { nameof(MemoryDate) }));
        }
        
        // Validar existência da data
        if (!Heloilo.Application.Helpers.ValidationHelper.IsValidDate(MemoryDate.Day, MemoryDate.Month, MemoryDate.Year))
        {
            results.Add(new ValidationResult("Data inválida", new[] { nameof(MemoryDate) }));
        }
        
        // Validação de tags
        if (Tags != null)
        {
            foreach (var tag in Tags)
            {
                if (!string.IsNullOrWhiteSpace(tag) && tag.Length > 50)
                {
                    results.Add(new ValidationResult("Cada tag deve ter no máximo 50 caracteres", new[] { nameof(Tags) }));
                }
            }
        }
        
        return results;
    }
}


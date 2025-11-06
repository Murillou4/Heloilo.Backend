using System.ComponentModel.DataAnnotations;
using Heloilo.Application.Helpers;

namespace Heloilo.Application.DTOs.Wish;

public class UpdateWishDto : IValidatableObject
{
    [MaxLength(500, ErrorMessage = "Título deve ter no máximo 500 caracteres")]
    public string? Title { get; set; }

    [MaxLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string? Description { get; set; }

    [MaxLength(1000, ErrorMessage = "URL deve ter no máximo 1000 caracteres")]
    [Url(ErrorMessage = "URL deve estar em um formato válido (HTTP ou HTTPS)")]
    public string? LinkUrl { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "ID de categoria inválido")]
    public long? CategoryId { get; set; }

    [Range(1, 5, ErrorMessage = "Nível de importância deve estar entre 1 e 5")]
    public int? ImportanceLevel { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(LinkUrl) && !ValidationHelper.IsValidUrl(LinkUrl))
        {
            yield return new ValidationResult(
                "URL deve usar protocolo HTTP ou HTTPS",
                new[] { nameof(LinkUrl) }
            );
        }
    }
}


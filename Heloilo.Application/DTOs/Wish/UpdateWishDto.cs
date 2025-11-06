using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Wish;

public class UpdateWishDto
{
    [MaxLength(500, ErrorMessage = "Título deve ter no máximo 500 caracteres")]
    public string? Title { get; set; }

    [MaxLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
    public string? Description { get; set; }

    [MaxLength(500, ErrorMessage = "URL deve ter no máximo 500 caracteres")]
    [Url(ErrorMessage = "URL deve estar em um formato válido")]
    public string? LinkUrl { get; set; }

    [Range(1, long.MaxValue, ErrorMessage = "ID de categoria inválido")]
    public long? CategoryId { get; set; }

    [Range(1, 5, ErrorMessage = "Nível de importância deve estar entre 1 e 5")]
    public int? ImportanceLevel { get; set; }
}


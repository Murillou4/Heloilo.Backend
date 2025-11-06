using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Story;

public class CreateStoryPageDto
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(500, ErrorMessage = "Título deve ter no máximo 500 caracteres")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000, ErrorMessage = "Conteúdo deve ter no máximo 5000 caracteres")]
    public string? Content { get; set; }

    [Required(ErrorMessage = "Data da página é obrigatória")]
    public DateOnly PageDate { get; set; }
}


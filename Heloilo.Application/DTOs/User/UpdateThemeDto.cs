using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.User;

public class UpdateThemeDto
{
    [Required(ErrorMessage = "Cor do tema é obrigatória")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Cor deve estar no formato hexadecimal (#RRGGBB)")]
    public string ThemeColor { get; set; } = string.Empty;
}


using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.User;

public class UpdatePasswordDto
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Nova senha deve ter no mínimo 6 caracteres")]
    [MaxLength(100, ErrorMessage = "Nova senha deve ter no máximo 100 caracteres")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare("NewPassword", ErrorMessage = "A confirmação de senha não confere com a nova senha")]
    public string ConfirmPassword { get; set; } = string.Empty;
}


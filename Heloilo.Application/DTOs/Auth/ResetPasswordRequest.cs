using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required(ErrorMessage = "Token é obrigatório")]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    public string NewPassword { get; set; } = string.Empty;
}


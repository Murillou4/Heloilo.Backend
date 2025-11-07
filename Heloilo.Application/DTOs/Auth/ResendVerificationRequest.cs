using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Auth;

public class ResendVerificationRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string Email { get; set; } = string.Empty;
}


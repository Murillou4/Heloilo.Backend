using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Auth;

public class VerifyEmailRequest
{
    [Required(ErrorMessage = "Token é obrigatório")]
    public string Token { get; set; } = string.Empty;
}


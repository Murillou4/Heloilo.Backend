using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [MaxLength(255, ErrorMessage = "Email deve ter no máximo 255 caracteres")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    [MaxLength(100, ErrorMessage = "Senha deve ter no máximo 100 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nome é obrigatório")]
    [MaxLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "Apelido deve ter no máximo 100 caracteres")]
    public string? Nickname { get; set; }
}


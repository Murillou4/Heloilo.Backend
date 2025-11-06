using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.User;

public class UpdateUserDto
{
    [MaxLength(255, ErrorMessage = "Nome deve ter no máximo 255 caracteres")]
    public string? Name { get; set; }

    [MaxLength(100, ErrorMessage = "Apelido deve ter no máximo 100 caracteres")]
    public string? Nickname { get; set; }
}


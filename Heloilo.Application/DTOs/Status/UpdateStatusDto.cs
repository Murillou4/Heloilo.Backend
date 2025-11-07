using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Status;

public class UpdateStatusDto
{
    [Required(ErrorMessage = "Status é obrigatório")]
    [MaxLength(500, ErrorMessage = "Status deve ter no máximo 500 caracteres")]
    public string CurrentStatus { get; set; } = string.Empty;
}


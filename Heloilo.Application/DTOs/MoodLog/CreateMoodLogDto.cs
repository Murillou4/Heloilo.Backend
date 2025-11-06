using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.MoodLog;

public class CreateMoodLogDto
{
    [Required(ErrorMessage = "Tipo de humor é obrigatório")]
    [Range(1, long.MaxValue, ErrorMessage = "Tipo de humor inválido")]
    public long MoodTypeId { get; set; }

    [MaxLength(2000, ErrorMessage = "Comentário deve ter no máximo 2000 caracteres")]
    public string? Comment { get; set; }

    public DateOnly? LogDate { get; set; }
}


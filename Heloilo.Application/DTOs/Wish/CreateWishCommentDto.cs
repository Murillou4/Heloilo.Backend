using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Wish;

public class CreateWishCommentDto
{
    [Required(ErrorMessage = "Conteúdo é obrigatório")]
    [MaxLength(2000, ErrorMessage = "Comentário deve ter no máximo 2000 caracteres")]
    public string Content { get; set; } = string.Empty;
}


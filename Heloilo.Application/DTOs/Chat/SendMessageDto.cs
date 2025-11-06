using System.ComponentModel.DataAnnotations;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.DTOs.Chat;

public class SendMessageDto
{
    [MaxLength(5000, ErrorMessage = "Conteúdo da mensagem deve ter no máximo 5000 caracteres")]
    public string? Content { get; set; }

    [Required(ErrorMessage = "Tipo de mensagem é obrigatório")]
    public MessageType MessageType { get; set; } = MessageType.Text;
}


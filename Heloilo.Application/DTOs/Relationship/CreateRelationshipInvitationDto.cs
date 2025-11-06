using System.ComponentModel.DataAnnotations;

namespace Heloilo.Application.DTOs.Relationship;

public class CreateRelationshipInvitationDto
{
    [Required(ErrorMessage = "Email do parceiro é obrigatório")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    public string PartnerEmail { get; set; } = string.Empty;
}


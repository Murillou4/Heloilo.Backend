using Heloilo.Domain.Models.Entities;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Heloilo.Application.Helpers;

public static class RelationshipValidationHelper
{
    /// <summary>
    /// Valida se o usuário pertence ao relacionamento ativo.
    /// </summary>
    public static async Task<Relationship?> ValidateUserRelationshipAsync(HeloiloDbContext context, long userId)
    {
        var relationship = await context.Relationships
            .FirstOrDefaultAsync(r => 
                (r.User1Id == userId || r.User2Id == userId) && 
                r.IsActive && 
                r.DeletedAt == null);

        return relationship;
    }

    /// <summary>
    /// Valida se o usuário pertence ao relacionamento e retorna o relacionamento.
    /// Lança exceção se não encontrar.
    /// </summary>
    public static async Task<Relationship> ValidateUserRelationshipOrThrowAsync(HeloiloDbContext context, long userId)
    {
        var relationship = await ValidateUserRelationshipAsync(context, userId);
        
        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado ou inativo");
        }

        return relationship;
    }

    /// <summary>
    /// Obtém o ID do parceiro do usuário no relacionamento.
    /// </summary>
    public static long GetPartnerId(Relationship relationship, long userId)
    {
        return relationship.User1Id == userId ? relationship.User2Id : relationship.User1Id;
    }

    /// <summary>
    /// Valida se o usuário tem permissão para acessar um recurso do relacionamento.
    /// </summary>
    public static async Task<bool> ValidateRelationshipAccessAsync(HeloiloDbContext context, long userId, long relationshipId)
    {
        var relationship = await context.Relationships
            .FirstOrDefaultAsync(r => 
                r.Id == relationshipId &&
                (r.User1Id == userId || r.User2Id == userId) && 
                r.IsActive && 
                r.DeletedAt == null);

        return relationship != null;
    }

    /// <summary>
    /// Valida se o usuário tem permissão para acessar um recurso do relacionamento.
    /// Lança exceção se não tiver permissão.
    /// </summary>
    public static async Task ValidateRelationshipAccessOrThrowAsync(HeloiloDbContext context, long userId, long relationshipId)
    {
        var hasAccess = await ValidateRelationshipAccessAsync(context, userId, relationshipId);
        
        if (!hasAccess)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este recurso");
        }
    }
}


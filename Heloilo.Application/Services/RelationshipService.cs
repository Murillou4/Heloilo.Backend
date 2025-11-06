using Heloilo.Application.DTOs.Relationship;
using Heloilo.Application.Helpers;
using Heloilo.Application.Interfaces;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Entities;
using Heloilo.Domain.Models.Enums;
using Heloilo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Heloilo.Application.Services;

public class RelationshipService : IRelationshipService
{
    private readonly HeloiloDbContext _context;
    private readonly ILogger<RelationshipService> _logger;

    public RelationshipService(HeloiloDbContext context, ILogger<RelationshipService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RelationshipInvitationDto> SendInvitationAsync(long userId, CreateRelationshipInvitationDto dto)
    {
        // Verificar se usuário já tem relacionamento ativo
        var hasActiveRelationship = await _context.Relationships
            .AnyAsync(r => (r.User1Id == userId || r.User2Id == userId) && r.IsActive && r.DeletedAt == null);

        if (hasActiveRelationship)
        {
            throw new InvalidOperationException("Você já possui um relacionamento ativo. Desvincule-se primeiro para criar um novo relacionamento.");
        }

        // Buscar parceiro por email
        var partner = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.PartnerEmail && u.DeletedAt == null);

        if (partner == null)
        {
            throw new KeyNotFoundException("Usuário com esse email não encontrado");
        }

        if (partner.Id == userId)
        {
            throw new InvalidOperationException("Não é possível enviar convite para você mesmo");
        }

        // Verificar se já existe convite pendente
        var existingInvitation = await _context.RelationshipInvitations
            .FirstOrDefaultAsync(i =>
                ((i.SenderId == userId && i.ReceiverId == partner.Id) ||
                 (i.SenderId == partner.Id && i.ReceiverId == userId)) &&
                i.Status == InvitationStatus.Pending);

        if (existingInvitation != null)
        {
            throw new InvalidOperationException("Já existe um convite pendente entre vocês");
        }

        // Criar convite
        var invitation = new RelationshipInvitation
        {
            SenderId = userId,
            ReceiverId = partner.Id,
            Status = InvitationStatus.Pending,
            SentAt = DateTime.UtcNow
        };

        _context.RelationshipInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        // Buscar dados completos para retornar
        var invitationWithDetails = await _context.RelationshipInvitations
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .FirstOrDefaultAsync(i => i.Id == invitation.Id);

        return MapInvitationToDto(invitationWithDetails!);
    }

    public async Task<PagedResult<RelationshipInvitationDto>> GetPendingInvitationsAsync(long userId, int page = 1, int pageSize = 20)
    {
        // Validar paginação
        (page, pageSize) = ValidationHelper.ValidatePagination(page, pageSize, defaultPageSize: 20, maxPageSize: 100);

        var query = _context.RelationshipInvitations
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .Where(i => i.ReceiverId == userId && i.Status == InvitationStatus.Pending)
            .OrderByDescending(i => i.SentAt);

        var totalItems = await query.CountAsync();

        var invitations = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<RelationshipInvitationDto>
        {
            Items = invitations.Select(MapInvitationToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems
        };
    }

    public async Task<RelationshipDto> AcceptInvitationAsync(long userId, long invitationId)
    {
        var invitation = await _context.RelationshipInvitations
            .Include(i => i.Sender)
            .Include(i => i.Receiver)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            throw new KeyNotFoundException("Convite não encontrado");
        }

        if (invitation.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para aceitar este convite");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Convite já foi processado");
        }

        // Verificar se algum dos usuários já tem relacionamento ativo
        var user1HasRelationship = await _context.Relationships
            .AnyAsync(r => (r.User1Id == invitation.SenderId || r.User2Id == invitation.SenderId) && r.IsActive && r.DeletedAt == null);

        var user2HasRelationship = await _context.Relationships
            .AnyAsync(r => (r.User1Id == invitation.ReceiverId || r.User2Id == invitation.ReceiverId) && r.IsActive && r.DeletedAt == null);

        if (user1HasRelationship || user2HasRelationship)
        {
            throw new InvalidOperationException("Um dos usuários já possui um relacionamento ativo");
        }

        // Criar relacionamento
        var relationship = new Relationship
        {
            User1Id = invitation.SenderId,
            User2Id = invitation.ReceiverId,
            IsActive = true,
            CelebrationType = CelebrationType.Annual
        };

        _context.Relationships.Add(relationship);

        // Atualizar convite
        invitation.Status = InvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;

        // Criar registros de initial setup
        var setup1 = new InitialSetup
        {
            RelationshipId = relationship.Id,
            UserId = invitation.SenderId,
            IsCompleted = false,
            IsSkipped = false
        };

        var setup2 = new InitialSetup
        {
            RelationshipId = relationship.Id,
            UserId = invitation.ReceiverId,
            IsCompleted = false,
            IsSkipped = false
        };

        _context.InitialSetups.AddRange(setup1, setup2);

        await _context.SaveChangesAsync();

        return await GetRelationshipDtoAsync(relationship.Id);
    }

    public async Task<bool> RejectInvitationAsync(long userId, long invitationId)
    {
        var invitation = await _context.RelationshipInvitations
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
        {
            throw new KeyNotFoundException("Convite não encontrado");
        }

        if (invitation.ReceiverId != userId)
        {
            throw new UnauthorizedAccessException("Você não tem permissão para rejeitar este convite");
        }

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new InvalidOperationException("Convite já foi processado");
        }

        invitation.Status = InvitationStatus.Rejected;
        invitation.RespondedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RelationshipDto?> GetCurrentRelationshipAsync(long userId)
    {
        var relationship = await _context.Relationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship == null)
        {
            return null;
        }

        return await GetRelationshipDtoAsync(relationship.Id);
    }

    public async Task<int> GetDaysTogetherAsync(long relationshipId)
    {
        var relationship = await _context.Relationships.FindAsync(relationshipId);
        if (relationship == null || !relationship.RelationshipStartDate.HasValue)
        {
            return 0;
        }

        var startDate = relationship.RelationshipStartDate.Value;
        var today = DateOnly.FromDateTime(DateTime.Today);
        return today.DayNumber - startDate.DayNumber;
    }

    public async Task<RelationshipDto> UpdateRelationshipConfigurationAsync(long userId, UpdateRelationshipConfigurationDto dto)
    {
        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        // Validar datas
        if (dto.MetDate.HasValue)
        {
            if (dto.MetDate.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException("Data de encontro não pode ser futura");
            }

            if (dto.MetDate.Value.Year < 1900)
            {
                throw new ArgumentException("Data de encontro não pode ser anterior a 1900");
            }

            relationship.MetDate = dto.MetDate;
        }

        if (dto.MetLocation != null)
        {
            relationship.MetLocation = dto.MetLocation;
        }

        if (dto.RelationshipStartDate.HasValue)
        {
            if (dto.RelationshipStartDate.Value > DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException("Data de início do relacionamento não pode ser futura");
            }

            if (dto.RelationshipStartDate.Value.Year < 1900)
            {
                throw new ArgumentException("Data de início não pode ser anterior a 1900");
            }

            relationship.RelationshipStartDate = dto.RelationshipStartDate;
        }

        if (dto.CelebrationType.HasValue)
        {
            relationship.CelebrationType = dto.CelebrationType.Value;
        }

        await _context.SaveChangesAsync();

        return await GetRelationshipDtoAsync(relationship.Id);
    }

    public async Task<bool> RequestUnlinkAsync(long userId)
    {
        // Em produção, implementar lógica de solicitação de desvinculação
        // Por enquanto, apenas retornar true
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> ConfirmUnlinkAsync(long userId)
    {
        var relationship = await _context.Relationships
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        // Soft delete do relacionamento
        relationship.IsActive = false;
        relationship.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<InitialSetupStatusDto> GetInitialSetupStatusAsync(long userId)
    {
        var relationship = await _context.Relationships
            .Include(r => r.InitialSetups)
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship == null)
        {
            return new InitialSetupStatusDto
            {
                IsCompleted = false,
                IsSkipped = false,
                CurrentUserCompleted = false,
                PartnerCompleted = false,
                CanAccessApp = false
            };
        }

        var userSetup = relationship.InitialSetups.FirstOrDefault(s => s.UserId == userId);
        var partnerSetup = relationship.InitialSetups.FirstOrDefault(s => s.UserId != userId);

        var currentUserCompleted = userSetup != null && (userSetup.IsCompleted || userSetup.IsSkipped);
        var partnerCompleted = partnerSetup != null && (partnerSetup.IsCompleted || partnerSetup.IsSkipped);

        return new InitialSetupStatusDto
        {
            IsCompleted = currentUserCompleted && partnerCompleted,
            IsSkipped = userSetup?.IsSkipped ?? false,
            CurrentUserCompleted = currentUserCompleted,
            PartnerCompleted = partnerCompleted,
            CanAccessApp = currentUserCompleted && partnerCompleted
        };
    }

    public async Task<bool> CompleteInitialSetupAsync(long userId)
    {
        var relationship = await _context.Relationships
            .Include(r => r.InitialSetups)
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        var setup = relationship.InitialSetups.FirstOrDefault(s => s.UserId == userId);
        if (setup == null)
        {
            throw new KeyNotFoundException("Configuração inicial não encontrada");
        }

        setup.IsCompleted = true;
        setup.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SkipInitialSetupAsync(long userId)
    {
        var relationship = await _context.Relationships
            .Include(r => r.InitialSetups)
            .FirstOrDefaultAsync(r =>
                (r.User1Id == userId || r.User2Id == userId) &&
                r.IsActive && r.DeletedAt == null);

        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        var setup = relationship.InitialSetups.FirstOrDefault(s => s.UserId == userId);
        if (setup == null)
        {
            throw new KeyNotFoundException("Configuração inicial não encontrada");
        }

        setup.IsSkipped = true;
        setup.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<RelationshipDto> GetRelationshipDtoAsync(long relationshipId)
    {
        var relationship = await _context.Relationships
            .Include(r => r.User1)
            .Include(r => r.User2)
            .FirstOrDefaultAsync(r => r.Id == relationshipId);

        if (relationship == null)
        {
            throw new KeyNotFoundException("Relacionamento não encontrado");
        }

        var daysTogether = await GetDaysTogetherAsync(relationshipId);

        return new RelationshipDto
        {
            Id = relationship.Id,
            User1Id = relationship.User1Id,
            User1Name = relationship.User1.Name,
            User2Id = relationship.User2Id,
            User2Name = relationship.User2.Name,
            MetDate = relationship.MetDate,
            MetLocation = relationship.MetLocation,
            RelationshipStartDate = relationship.RelationshipStartDate,
            CelebrationType = relationship.CelebrationType,
            IsActive = relationship.IsActive,
            DaysTogether = daysTogether,
            CreatedAt = relationship.CreatedAt
        };
    }

    private static RelationshipInvitationDto MapInvitationToDto(RelationshipInvitation invitation)
    {
        return new RelationshipInvitationDto
        {
            Id = invitation.Id,
            SenderId = invitation.SenderId,
            SenderName = invitation.Sender.Name,
            SenderNickname = invitation.Sender.Nickname,
            ReceiverId = invitation.ReceiverId,
            ReceiverName = invitation.Receiver.Name,
            ReceiverNickname = invitation.Receiver.Nickname,
            Status = invitation.Status,
            SentAt = invitation.SentAt,
            RespondedAt = invitation.RespondedAt
        };
    }
}


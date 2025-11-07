using Heloilo.Application.DTOs.Relationship;
using Heloilo.Domain.Models.Common;

namespace Heloilo.Application.Interfaces;

public interface IRelationshipService
{
    Task<RelationshipInvitationDto> SendInvitationAsync(long userId, CreateRelationshipInvitationDto dto);
    Task<PagedResult<RelationshipInvitationDto>> GetPendingInvitationsAsync(long userId, int page = 1, int pageSize = 20);
    Task<RelationshipDto> AcceptInvitationAsync(long userId, long invitationId);
    Task<bool> RejectInvitationAsync(long userId, long invitationId);
    Task<RelationshipDto?> GetCurrentRelationshipAsync(long userId);
    Task<int> GetDaysTogetherAsync(long relationshipId);
    Task<RelationshipDto> UpdateRelationshipConfigurationAsync(long userId, UpdateRelationshipConfigurationDto dto);
    Task<bool> RequestUnlinkAsync(long userId);
    Task<bool> ConfirmUnlinkAsync(long userId);
    Task<InitialSetupStatusDto> GetInitialSetupStatusAsync(long userId);
    Task<bool> CompleteInitialSetupAsync(long userId);
    Task<bool> SkipInitialSetupAsync(long userId);
}


using Heloilo.Application.DTOs.Status;
using Heloilo.Domain.Models.Common;

namespace Heloilo.Application.Interfaces;

public interface IUserStatusService
{
    Task<UserStatusDto> GetCurrentStatusAsync(long userId);
    Task<UserStatusDto> UpdateStatusAsync(long userId, UpdateStatusDto dto);
    Task<UserStatusDto?> GetPartnerStatusAsync(long userId);
    Task<PagedResult<UserStatusDto>> GetStatusHistoryAsync(long userId, DateOnly? date = null, int page = 1, int pageSize = 20);
    Task<PagedResult<UserStatusDto>> GetPartnerStatusHistoryAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, int page = 1, int pageSize = 20);
    Task<bool> IsStatusExpiredAsync(long userId);
}


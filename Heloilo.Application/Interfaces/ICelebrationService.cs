using Heloilo.Application.DTOs.Celebration;

namespace Heloilo.Application.Interfaces;

public interface ICelebrationService
{
    Task<AnniversaryInfoDto> GetAnniversaryInfoAsync(long userId);
    Task<List<object>> GetUpcomingCelebrationsAsync(long userId);
}


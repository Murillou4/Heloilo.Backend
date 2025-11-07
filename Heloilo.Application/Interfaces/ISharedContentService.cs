using Heloilo.Application.DTOs.Shared;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.Interfaces;

public interface ISharedContentService
{
    Task<SharedContentDto> CreateShareLinkAsync(long userId, ContentType contentType, long contentId, int? expirationDays = null);
    Task<SharedContentDto?> GetSharedContentAsync(string token);
    Task<bool> RevokeShareLinkAsync(long userId, string token);
    Task<bool> IncrementAccessCountAsync(string token);
}


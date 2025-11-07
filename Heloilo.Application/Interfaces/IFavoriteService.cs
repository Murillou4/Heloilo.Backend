using Heloilo.Application.DTOs.Favorite;
using Heloilo.Domain.Models.Common;
using Heloilo.Domain.Models.Enums;

namespace Heloilo.Application.Interfaces;

public interface IFavoriteService
{
    Task<bool> AddFavoriteAsync(long userId, ContentType contentType, long contentId);
    Task<bool> RemoveFavoriteAsync(long userId, ContentType contentType, long contentId);
    Task<bool> IsFavoriteAsync(long userId, ContentType contentType, long contentId);
    Task<PagedResult<FavoriteDto>> GetFavoritesAsync(long userId, ContentType? contentType = null, int page = 1, int pageSize = 20);
}


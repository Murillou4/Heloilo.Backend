using Heloilo.Application.DTOs.Shared;

namespace Heloilo.Application.Interfaces;

public interface IMediaService
{
    Task<MediaFileResult?> GetUserPhotoAsync(long userId, long requestingUserId);
    Task<MediaFileResult?> GetMemoryMediaAsync(long mediaId, long userId);
    Task<MediaFileResult?> GetMessageMediaAsync(long mediaId, long userId);
    Task<MediaFileResult?> GetStoryPageImageAsync(long pageId, long userId);
    Task<MediaFileResult?> GetWishImageAsync(long wishId, long userId);
}


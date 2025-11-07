namespace Heloilo.Application.Interfaces;

public interface IMediaService
{
    Task<byte[]?> GetUserPhotoAsync(long userId, long requestingUserId);
    Task<byte[]?> GetMemoryMediaAsync(long mediaId, long userId);
    Task<byte[]?> GetMessageMediaAsync(long mediaId, long userId);
    Task<byte[]?> GetStoryPageImageAsync(long pageId, long userId);
    Task<byte[]?> GetWishImageAsync(long wishId, long userId);
}


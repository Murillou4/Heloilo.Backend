using Heloilo.Application.DTOs.Story;
using Heloilo.Domain.Models.Common;
using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Interfaces;

public interface IStoryService
{
    Task<PagedResult<StoryPageDto>> GetStoryPagesAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20);
    Task<StoryPageDto> GetStoryPageByIdAsync(long pageId, long userId);
    Task<StoryPageDto> CreateStoryPageAsync(long userId, CreateStoryPageDto dto, IFormFile? image = null);
    Task<StoryPageDto> UpdateStoryPageAsync(long pageId, long userId, CreateStoryPageDto dto, IFormFile? image = null);
    Task<bool> DeleteStoryPageAsync(long pageId, long userId);
    Task<byte[]?> GetStoryPageImageAsync(long pageId, long userId);
}


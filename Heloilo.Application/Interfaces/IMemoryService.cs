using Heloilo.Application.DTOs.Memory;
using Heloilo.Domain.Models.Common;
using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Interfaces;

public interface IMemoryService
{
    Task<PagedResult<MemoryDto>> GetMemoriesAsync(long userId, DateOnly? startDate = null, DateOnly? endDate = null, List<string>? tags = null, string? search = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20);
    Task<MemoryDto> GetMemoryByIdAsync(long memoryId, long userId);
    Task<MemoryDto> CreateMemoryAsync(long userId, CreateMemoryDto dto, List<IFormFile>? media = null);
    Task<MemoryDto> UpdateMemoryAsync(long memoryId, long userId, CreateMemoryDto dto);
    Task<bool> DeleteMemoryAsync(long memoryId, long userId);
    Task<string> AddMediaAsync(long memoryId, long userId, IFormFile file);
    Task<bool> DeleteMediaAsync(long memoryId, long mediaId, long userId);
    Task<List<string>> GetTagsAsync(long userId);
}


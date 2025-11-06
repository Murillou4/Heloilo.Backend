using Heloilo.Application.DTOs.Wish;
using Heloilo.Domain.Models.Common;
using Microsoft.AspNetCore.Http;

namespace Heloilo.Application.Interfaces;

public interface IWishService
{
    Task<PagedResult<WishDto>> GetWishesAsync(long userId, long? categoryId = null, string? search = null, string? sortBy = null, string? sortOrder = null, int page = 1, int pageSize = 20);
    Task<WishDto> GetWishByIdAsync(long wishId, long userId);
    Task<WishDto> CreateWishAsync(long userId, CreateWishDto dto, IFormFile? image = null);
    Task<WishDto> UpdateWishAsync(long wishId, long userId, UpdateWishDto dto, IFormFile? image = null);
    Task<bool> DeleteWishAsync(long wishId, long userId);
    Task<List<WishCategoryDto>> GetCategoriesAsync();
    Task<PagedResult<WishCommentDto>> GetWishCommentsAsync(long wishId, int page = 1, int pageSize = 20);
    Task<WishCommentDto> AddCommentAsync(long wishId, long userId, CreateWishCommentDto dto);
    Task<WishCommentDto> UpdateCommentAsync(long commentId, long userId, CreateWishCommentDto dto);
    Task<bool> DeleteCommentAsync(long commentId, long userId);
}


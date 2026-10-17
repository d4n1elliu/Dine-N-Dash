using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Browsing, searching and managing restaurants
/// </summary>
public interface IRestaurantService
{
    /// <summary>
    /// A null filter is ignored rather than matched. Throws BadRequestException for an invalid page or page size
    /// </summary>
    Task<PagedResult<Restaurant>> SearchAsync(string? name, CuisineType? cuisine, string? suburb, string? sortBy, int page, int pageSize);
    /// <summary>
    /// Returns the highest rated restaurants a page at a time, with unreviewed ones after them
    /// </summary>
    Task<PagedResult<Restaurant>> GetRecommendedAsync(int page, int pageSize);
    /// <summary>
    /// Throws NotFoundException rather than returning null, unlike the repository
    /// </summary>
    Task<Restaurant> GetByIdAsync(int id);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
}

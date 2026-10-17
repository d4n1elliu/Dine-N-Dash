using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Browsing, searching and managing restaurants
/// </summary>
public interface IRestaurantService
{
    /// <summary>
    /// A null filter is ignored rather than matched
    /// </summary>
    Task<IEnumerable<Restaurant>> SearchAsync(string? name, CuisineType? cuisine, string? suburb, string? sortBy);
    /// <summary>
    /// Returns the highest rated restaurants, with unreviewed ones filling any remaining spots
    /// </summary>
    Task<IEnumerable<Restaurant>> GetRecommendedAsync(int count);
    /// <summary>
    /// Throws NotFoundException rather than returning null, unlike the repository
    /// </summary>
    Task<Restaurant> GetByIdAsync(int id);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
}

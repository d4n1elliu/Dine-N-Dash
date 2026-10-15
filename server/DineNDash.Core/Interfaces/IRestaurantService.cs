using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Not implemented yet; the restaurant endpoints are still to come
/// </summary>
public interface IRestaurantService
{
    /// <summary>
    /// A null filter is ignored rather than matched
    /// </summary>
    Task<IEnumerable<Restaurant>> SearchAsync(string? name, CuisineType? cuisine, string? suburb, string? sortBy);
    /// <summary>
    /// Throws NotFoundException rather than returning null, unlike the repository
    /// </summary>
    Task<Restaurant> GetByIdAsync(int id);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
}
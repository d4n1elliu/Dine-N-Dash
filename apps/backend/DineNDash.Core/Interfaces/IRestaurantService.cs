using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IRestaurantService
{
    Task<IEnumerable<Restaurant>> SearchAsync(string? name, CuisineType? cuisine, string? suburb, string? sortBy);
    Task<Restaurant> GetByIdAsync(int id);
    Task<Restaurant> CreateAsync(Restaurant restaurant);
    Task<Restaurant> UpdateAsync(Restaurant restaurant);
}
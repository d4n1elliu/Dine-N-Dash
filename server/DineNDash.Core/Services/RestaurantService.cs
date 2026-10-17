using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;
/// <summary>
/// Restaurant rules: partial name search, filtering, sorting and recommendations by rating
/// </summary>
public class RestaurantService : IRestaurantService
{
    private IRepository<Restaurant> _restaurants;
    private IRepository<Review> _reviews;

    public RestaurantService(IRepository<Restaurant> restaurants, IRepository<Review> reviews)
    {
        _restaurants = restaurants;
        _reviews = reviews;
    }

    /// <summary>
    /// Matches names and suburbs partially and ignoring case, then sorts by rating, price or name (the default)
    /// </summary>
    public async Task<IEnumerable<Restaurant>> SearchAsync(string? name, CuisineType? cuisine, string? suburb, string? sortBy)
    {
        var allRestaurants = await _restaurants.GetAllAsync();
        var matches = allRestaurants.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            matches = matches.Where(restaurant => restaurant.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (cuisine != null)
        {
            matches = matches.Where(restaurant => restaurant.Cuisine == cuisine);
        }

        if (!string.IsNullOrWhiteSpace(suburb))
        {
            matches = matches.Where(restaurant => restaurant.Suburb.Contains(suburb.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        switch (sortBy?.Trim().ToLowerInvariant())
        {
            case null or "" or "name":
                return matches.OrderBy(restaurant => restaurant.Name).ToList();
            case "price":
                return matches.OrderBy(restaurant => restaurant.PriceLevel)
                              .ThenBy(restaurant => restaurant.Name)
                              .ToList();
            case "rating":
                var averages = await GetAverageRatingsAsync();
                return matches.OrderByDescending(restaurant => averages.GetValueOrDefault(restaurant.Id))
                              .ThenBy(restaurant => restaurant.Name)
                              .ToList();
            default:
                throw new BadRequestException("Sort must be name, price or rating.");
        }
    }

    /// <summary>
    /// Ranks by average rating, then by number of reviews, then newest first for restaurants with no reviews
    /// </summary>
    public async Task<IEnumerable<Restaurant>> GetRecommendedAsync(int count)
    {
        if (count <= 0)
        {
            throw new BadRequestException("Count must be greater than 0.");
        }

        var allRestaurants = await _restaurants.GetAllAsync();
        var allReviews = await _reviews.GetAllAsync();
        var ratings = allReviews.GroupBy(review => review.RestaurantId)
                                .ToDictionary(group => group.Key,
                                              group => (Average: group.Average(review => review.Rating), Count: group.Count()));

        return allRestaurants.OrderByDescending(restaurant => ratings.GetValueOrDefault(restaurant.Id).Average)
                             .ThenByDescending(restaurant => ratings.GetValueOrDefault(restaurant.Id).Count)
                             .ThenByDescending(restaurant => restaurant.CreatedAt)
                             .Take(count)
                             .ToList();
    }

    /// <summary>
    /// Returns one restaurant or throws when it does not exist
    /// </summary>
    public async Task<Restaurant> GetByIdAsync(int id)
    {
        return await _restaurants.GetByIdAsync(id) ?? throw new NotFoundException("Unable to find restaurant.");
    }

    /// <summary>
    /// Validates and stores a new restaurant
    /// </summary>
    public async Task<Restaurant> CreateAsync(Restaurant restaurant)
    {
        restaurant.Validate();
        return await _restaurants.AddAsync(restaurant);
    }

    /// <summary>
    /// Saves changes to an existing restaurant, throwing when it does not exist
    /// </summary>
    public async Task<Restaurant> UpdateAsync(Restaurant restaurant)
    {
        var existing = await GetByIdAsync(restaurant.Id);

        existing.Name = restaurant.Name;
        existing.Description = restaurant.Description;
        existing.Cuisine = restaurant.Cuisine;
        existing.Address = restaurant.Address;
        existing.Suburb = restaurant.Suburb;
        existing.PriceLevel = restaurant.PriceLevel;
        existing.Capacity = restaurant.Capacity;
        existing.PhotoUrls = restaurant.PhotoUrls;
        existing.Validate();
        existing.UpdatedAt = DateTime.UtcNow;

        await _restaurants.UpdateAsync(existing);
        return existing;
    }

    private async Task<Dictionary<int, double>> GetAverageRatingsAsync()
    {
        var allReviews = await _reviews.GetAllAsync();
        return allReviews.GroupBy(review => review.RestaurantId)
                         .ToDictionary(group => group.Key, group => group.Average(review => review.Rating));
    }
}

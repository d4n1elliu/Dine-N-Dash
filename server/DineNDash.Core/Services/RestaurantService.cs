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
    public const int MaxPageSize = 50;

    private IRepository<Restaurant> _restaurants;
    private IRepository<Review> _reviews;

    public RestaurantService(IRepository<Restaurant> restaurants, IRepository<Review> reviews)
    {
        _restaurants = restaurants;
        _reviews = reviews;
    }

    /// <summary>
    /// Matches names and suburbs partially and ignoring case, sorts by rating, price or name (the default), then returns the requested page
    /// </summary>
    public async Task<PagedResult<Restaurant>> SearchAsync(string? name, CuisineType? cuisine, string? suburb, string? sortBy, int page, int pageSize) 
    { 
        ValidatePaging(page, pageSize);

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

        IEnumerable<Restaurant> sorted;
        switch (sortBy?.Trim().ToLowerInvariant()) 
        {
            case null or "" or "name":
                sorted = matches.OrderBy(restaurant => restaurant.Name);
                break;
            case "price":
                sorted = matches.OrderBy(restaurant => restaurant.PriceLevel)
                                .ThenBy(restaurant => restaurant.Name);
                break;
            case "rating":
                var averages = await GetAverageRatingsAsync();
                sorted = matches.OrderByDescending(restaurant => averages.GetValueOrDefault(restaurant.Id))
                                .ThenBy(restaurant => restaurant.Name);
                break;
            default:
                throw new BadRequestException("Sort must be name, price or rating.");
        }

        return ToPage(sorted.ToList(), page, pageSize);
    }

    /// <summary>
    /// Ranks by average rating, then by number of reviews, then newest first for restaurants with no reviews
    /// </summary>
    public async Task<PagedResult<Restaurant>> GetRecommendedAsync(int page, int pageSize) 
    { 
        ValidatePaging(page, pageSize);

        var allRestaurants = await _restaurants.GetAllAsync();
        var allReviews = await _reviews.GetAllAsync();
        var ratings = allReviews.GroupBy(review => review.RestaurantId)
                                .ToDictionary(group => group.Key,
                                              group => (Average: group.Average(review => review.Rating), Count: group.Count()));

        var ranked = allRestaurants.OrderByDescending(restaurant => ratings.GetValueOrDefault(restaurant.Id).Average)
                                   .ThenByDescending(restaurant => ratings.GetValueOrDefault(restaurant.Id).Count)
                                   .ThenByDescending(restaurant => restaurant.CreatedAt)
                                   .ToList();

        return ToPage(ranked, page, pageSize);
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

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new BadRequestException("Page must be 1 or greater.");
        }

        if (pageSize is < 1 or > MaxPageSize)
        {
            throw new BadRequestException($"Page size must be between 1 and {MaxPageSize}.");
        }
    }

    /// <summary>
    /// Cuts one page out of an already sorted list; a page past the end comes back empty
    /// </summary>
    private static PagedResult<Restaurant> ToPage(List<Restaurant> sorted, int page, int pageSize) => new()
    {
        Items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
        Page = page,
        PageSize = pageSize,
        TotalCount = sorted.Count
    };

    private async Task<Dictionary<int, double>> GetAverageRatingsAsync()
    {
        var allReviews = await _reviews.GetAllAsync();
        return allReviews.GroupBy(review => review.RestaurantId)
                         .ToDictionary(group => group.Key, group => group.Average(review => review.Rating));
    }
}

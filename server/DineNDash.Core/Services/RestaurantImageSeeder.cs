using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;
/// <summary>
/// Matches seed photos to restaurants by name and uploads them the same way an owner does from their dashboard
/// </summary>
public class RestaurantImageSeeder : IRestaurantImageSeeder
{
    private IRepository<Restaurant> _restaurants;
    private IRestaurantService _restaurantService;

    public RestaurantImageSeeder(IRepository<Restaurant> restaurants, IRestaurantService restaurantService)
    {
        _restaurants = restaurants;
        _restaurantService = restaurantService;
    }

    /// <summary>
    /// Uploads each photo to its restaurant unless the restaurant already has one, so a second run changes nothing
    /// </summary>
    public async Task<RestaurantImageSeedResult> SeedAsync(IEnumerable<RestaurantSeedImage> images)
    {
        var result = new RestaurantImageSeedResult();
        var allRestaurants = (await _restaurants.GetAllAsync()).ToList();

        foreach (var seed in images)
        {
            var matches = allRestaurants.Where(candidate =>
                string.Equals(candidate.Name.Trim(), seed.RestaurantName.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();

            if (matches.Count == 0)
            {
                result.Unmatched.Add(seed.RestaurantName);
                continue;
            }

            if (matches.Count > 1)
            {
                result.Ambiguous.Add(seed.RestaurantName);
                continue;
            }

            var restaurant = matches[0];
            if (restaurant.ProfileImageId != null)
            {
                result.Skipped++;
                continue;
            }

            await using var content = seed.Open();
            await _restaurantService.SetProfileImageAsync(restaurant.OwnerId, content, seed.FileName);
            result.Seeded++;
        }

        return result;
    }
}

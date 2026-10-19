using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Gives the sample restaurants their starting photo through the normal upload flow
/// </summary>
public interface IRestaurantImageSeeder
{
    /// <summary>
    /// Safe to run repeatedly: restaurants that already have a profile image are left alone
    /// </summary>
    Task<RestaurantImageSeedResult> SeedAsync(IEnumerable<RestaurantSeedImage> images);
}

using DineNDash.Core.Models;

namespace DineNDash.Core.Services;

/// <summary>
/// A restaurant's average rating and review count
/// </summary>
public readonly record struct RatingSummary(double Average, int Count);

/// <summary>
/// Rating calculations shared by the restaurant features
/// </summary>
public static class RatingStats
{
    /// <summary>
    /// Summarises reviews per restaurant id. Restaurants with no reviews are absent, so use GetValueOrDefault for zeros
    /// </summary>
    public static Dictionary<int, RatingSummary> ByRestaurant(IEnumerable<Review> reviews) =>
        reviews.GroupBy(review => review.RestaurantId)
               .ToDictionary(group => group.Key,
                             group => new RatingSummary(group.Average(review => review.Rating), group.Count()));
}

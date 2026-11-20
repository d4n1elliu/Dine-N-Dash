using DineNDash.Core.Models;

namespace DineNDash.Core.Services;

public readonly record struct RatingSummary(double Average, int Count);

public static class RatingStats
{
    /// <summary>
    /// Summarises reviews per restaurant id. Restaurants with no reviews are absent, so use GetValueOrDefault for zeros
    /// </summary>
    public static Dictionary<int, RatingSummary> ByRestaurant(IEnumerable<Review> reviews) =>
        reviews.GroupBy(review => review.RestaurantId)
               .ToDictionary(group => group.Key,
                             group => new RatingSummary(group.Average(review => review.Rating), group.Count()));

    /// <summary>
    /// Average rating across every review, or 0 when there are none
    /// </summary>
    public static double GlobalMean(IEnumerable<Review> reviews) =>
        reviews.Select(review => (double)review.Rating).DefaultIfEmpty(0).Average();
}

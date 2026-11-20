using DineNDash.Core.Models;

namespace DineNDash.Core.Services;

/// <summary>
/// A restaurant with the rating details used to compare it to others
/// </summary>
public record RestaurantFeatures(Restaurant Restaurant, double Rating, int ReviewCount);

/// <summary>
/// Measures how alike two restaurants are by cuisine, suburb, price level and average rating
/// </summary>
public static class RestaurantSimilarity
{
    public const double CuisineWeight = 4;
    public const double PriceWeight = 3;
    public const double SuburbWeight = 2;
    public const double RatingWeight = 1;

    private const double MaxPriceGap = 3;
    private const double MaxRatingGap = 4;

    /// <summary>
    /// Orders equally distant restaurants by most reviews, then name, then id
    /// </summary>
    public static IComparer<RestaurantFeatures> TieBreaker { get; } = Comparer<RestaurantFeatures>.Create((a, b) =>
    {
        int byReviews = b.ReviewCount.CompareTo(a.ReviewCount);
        if (byReviews != 0)
        {
            return byReviews;
        }

        int byName = string.Compare(a.Restaurant.Name, b.Restaurant.Name, StringComparison.OrdinalIgnoreCase);
        return byName != 0 ? byName : a.Restaurant.Id.CompareTo(b.Restaurant.Id);
    });

    /// <summary>
    /// Pairs a restaurant with its rating. An unreviewed restaurant gets the neutral rating so it is not treated as badly rated
    /// </summary>
    public static RestaurantFeatures ToFeatures(Restaurant restaurant, IReadOnlyDictionary<int, RatingSummary> ratings, double neutralRating) =>
        ratings.TryGetValue(restaurant.Id, out var summary)
            ? new RestaurantFeatures(restaurant, summary.Average, summary.Count)
            : new RestaurantFeatures(restaurant, neutralRating, 0);

    /// <summary>
    /// Weighted Euclidean distance, where 0 means identical and larger means less alike
    /// </summary>
    public static double Distance(RestaurantFeatures a, RestaurantFeatures b) =>
        Math.Sqrt(
            Weighted(CuisineWeight, CuisineGap(a, b)) +
            Weighted(SuburbWeight, SuburbGap(a, b)) +
            Weighted(PriceWeight, PriceGap(a, b)) +
            Weighted(RatingWeight, RatingGap(a, b)));

    // Cuisine and suburb are categories, so they are either the same (0) or not (1)
    private static double CuisineGap(RestaurantFeatures a, RestaurantFeatures b) =>
        a.Restaurant.Cuisine == b.Restaurant.Cuisine ? 0 : 1;

    private static double SuburbGap(RestaurantFeatures a, RestaurantFeatures b) =>
        string.Equals(a.Restaurant.Suburb.Trim(), b.Restaurant.Suburb.Trim(), StringComparison.OrdinalIgnoreCase) ? 0 : 1;

    private static double PriceGap(RestaurantFeatures a, RestaurantFeatures b) =>
        Math.Abs(a.Restaurant.PriceLevel - b.Restaurant.PriceLevel) / MaxPriceGap;

    private static double RatingGap(RestaurantFeatures a, RestaurantFeatures b) =>
        Math.Abs(a.Rating - b.Rating) / MaxRatingGap;

    private static double Weighted(double weight, double gap) => weight * gap * gap;
}

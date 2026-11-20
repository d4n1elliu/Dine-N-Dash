using DineNDash.Core.Models;
using DineNDash.Core.Services;

namespace DineNDash.Tests.Services;

public class RatingStatsTests
{
    private static Review Review(int restaurantId, int rating) => new() { RestaurantId = restaurantId, Rating = rating };

    [Test]
    public void ByRestaurant_AveragesAndCountsPerRestaurant()
    {
        var summaries = RatingStats.ByRestaurant([Review(1, 5), Review(1, 3), Review(2, 2)]);

        Assert.That(summaries[1], Is.EqualTo(new RatingSummary(4, 2)));
        Assert.That(summaries[2], Is.EqualTo(new RatingSummary(2, 1)));
    }

    [Test]
    public void ByRestaurant_NoReviews_ReturnsEmpty()
    {
        Assert.That(RatingStats.ByRestaurant([]), Is.Empty);
    }

    [Test]
    public void ByRestaurant_UnreviewedRestaurant_DefaultsToZero()
    {
        var summaries = RatingStats.ByRestaurant([Review(1, 4)]);

        Assert.That(summaries.GetValueOrDefault(99), Is.EqualTo(new RatingSummary(0, 0)));
    }

    [Test]
    public void GlobalMean_AveragesEveryReview()
    {
        Assert.That(RatingStats.GlobalMean([Review(1, 5), Review(1, 3), Review(2, 1)]), Is.EqualTo(3));
    }

    [Test]
    public void GlobalMean_NoReviews_IsZero()
    {
        Assert.That(RatingStats.GlobalMean([]), Is.EqualTo(0));
    }
}

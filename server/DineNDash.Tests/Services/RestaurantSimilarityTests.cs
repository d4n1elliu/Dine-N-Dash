using DineNDash.Core.Enums;
using DineNDash.Core.Models;
using DineNDash.Core.Services;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers the restaurant distance and its tie-breaker
/// </summary>
public class RestaurantSimilarityTests
{
    private static RestaurantFeatures Features(
        string name = "Place",
        CuisineType cuisine = CuisineType.Thai,
        string suburb = "Newtown",
        int priceLevel = 1,
        double rating = 4,
        int reviewCount = 10,
        int id = 0) =>
        new(new Restaurant { Id = id, Name = name, Cuisine = cuisine, Suburb = suburb, PriceLevel = priceLevel }, rating, reviewCount);

    [Test]
    public void Distance_IdenticalFeatures_IsZero()
    {
        Assert.That(RestaurantSimilarity.Distance(Features(), Features()), Is.EqualTo(0));
    }

    [Test]
    public void Distance_IsSymmetric()
    {
        var a = Features(cuisine: CuisineType.Thai, priceLevel: 1);
        var b = Features(cuisine: CuisineType.Italian, priceLevel: 3);

        Assert.That(RestaurantSimilarity.Distance(a, b), Is.EqualTo(RestaurantSimilarity.Distance(b, a)));
    }

    [Test]
    public void Distance_DifferentCuisine_IsGreaterThanZero()
    {
        var distance = RestaurantSimilarity.Distance(Features(), Features(cuisine: CuisineType.Italian));

        Assert.That(distance, Is.GreaterThan(0));
    }

    [Test]
    public void Distance_DifferentSuburb_IsGreaterThanZero()
    {
        var distance = RestaurantSimilarity.Distance(Features(), Features(suburb: "Glebe"));

        Assert.That(distance, Is.GreaterThan(0));
    }

    [Test]
    public void Distance_SameSuburbIgnoringCaseAndSpaces_IsZero()
    {
        var distance = RestaurantSimilarity.Distance(Features(suburb: "Newtown"), Features(suburb: "  newTOWN "));

        Assert.That(distance, Is.EqualTo(0));
    }

    [Test]
    public void Distance_LargerPriceGap_IsFurther()
    {
        var target = Features(priceLevel: 1);

        var oneStep = RestaurantSimilarity.Distance(target, Features(priceLevel: 2));
        var threeSteps = RestaurantSimilarity.Distance(target, Features(priceLevel: 4));

        Assert.That(oneStep, Is.GreaterThan(0));
        Assert.That(threeSteps, Is.GreaterThan(oneStep));
    }

    [Test]
    public void Distance_LargerRatingGap_IsFurther()
    {
        var target = Features(rating: 5);

        var small = RestaurantSimilarity.Distance(target, Features(rating: 4));
        var large = RestaurantSimilarity.Distance(target, Features(rating: 1));

        Assert.That(small, Is.GreaterThan(0));
        Assert.That(large, Is.GreaterThan(small));
    }

    [Test]
    public void Distance_CuisineMismatch_OutweighsOnePriceStep()
    {
        var target = Features();

        var otherCuisine = RestaurantSimilarity.Distance(target, Features(cuisine: CuisineType.Italian));
        var otherPrice = RestaurantSimilarity.Distance(target, Features(priceLevel: 2));

        Assert.That(otherCuisine, Is.GreaterThan(otherPrice));
    }

    [Test]
    public void Distance_CheapThaiInNewtown_IsCloserToCheapThaiThanExpensiveItalian()
    {
        var target = Features(cuisine: CuisineType.Thai, suburb: "Newtown", priceLevel: 1, rating: 4.5);
        var cheapThai = Features(cuisine: CuisineType.Thai, suburb: "Newtown", priceLevel: 1, rating: 4.2);
        var expensiveItalian = Features(cuisine: CuisineType.Italian, suburb: "Bondi", priceLevel: 4, rating: 4.5);

        Assert.That(
            RestaurantSimilarity.Distance(target, cheapThai),
            Is.LessThan(RestaurantSimilarity.Distance(target, expensiveItalian)));
    }

    [Test]
    public void TieBreaker_MoreReviewsFirst()
    {
        var popular = Features(name: "Zed", reviewCount: 20);
        var quiet = Features(name: "Alpha", reviewCount: 5);

        Assert.That(RestaurantSimilarity.TieBreaker.Compare(popular, quiet), Is.LessThan(0));
    }

    [Test]
    public void TieBreaker_SameReviewCount_OrdersByNameIgnoringCase()
    {
        var first = Features(name: "alpha", reviewCount: 5);
        var second = Features(name: "Beta", reviewCount: 5);

        Assert.That(RestaurantSimilarity.TieBreaker.Compare(first, second), Is.LessThan(0));
    }

    [Test]
    public void TieBreaker_SameReviewCountAndName_OrdersById()
    {
        var first = Features(name: "Twin", reviewCount: 5, id: 1);
        var second = Features(name: "Twin", reviewCount: 5, id: 2);

        Assert.That(RestaurantSimilarity.TieBreaker.Compare(first, second), Is.LessThan(0));
        Assert.That(RestaurantSimilarity.TieBreaker.Compare(second, first), Is.GreaterThan(0));
    }

    [Test]
    public void FindNearest_WithTieBreaker_OrdersEqualDistancesByReviewsThenName()
    {
        var target = Features(name: "Target");
        var fewReviews = Features(name: "Alpha", reviewCount: 2);
        var manyReviews = Features(name: "Zulu", reviewCount: 9);
        var sameCountLaterName = Features(name: "Beta", reviewCount: 2);

        var result = Knn.FindNearest(
            target,
            [fewReviews, sameCountLaterName, manyReviews],
            3,
            RestaurantSimilarity.Distance,
            RestaurantSimilarity.TieBreaker);

        Assert.That(result.Select(r => r.Restaurant.Name), Is.EqualTo(new[] { "Zulu", "Alpha", "Beta" }));
    }
}

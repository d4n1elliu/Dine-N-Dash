using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Models;
using DineNDash.Core.Services;
using DineNDash.Tests.Fakes;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers search, recommendations and lookups in RestaurantService
/// </summary>
public class RestaurantServiceTests
{
    private RestaurantService _restaurantService = null!;
    private InMemoryRepository<Restaurant> _restaurants = null!;
    private InMemoryRepository<Review> _reviews = null!;

    [SetUp]
    public void SetUp()
    {
        _restaurants = new InMemoryRepository<Restaurant>();
        _reviews = new InMemoryRepository<Review>();
        _restaurantService = new RestaurantService(_restaurants, _reviews);
    }

    private Task<Restaurant> AddRestaurant(string name, CuisineType cuisine = CuisineType.Other, string suburb = "Sydney", int priceLevel = 2) =>
        _restaurants.AddAsync(new Restaurant
        {
            Name = name,
            Cuisine = cuisine,
            Address = "1 Test St",
            Suburb = suburb,
            PriceLevel = priceLevel
        });

    private Task<Review> AddReview(int restaurantId, int dinerId, int rating) =>
        _reviews.AddAsync(new Review
        {
            RestaurantId = restaurantId,
            DinerId = dinerId,
            Rating = rating,
            Comment = "Test review"
        });

    [Test]
    public async Task Search_PartialNameIgnoringCase_ReturnsMatches()
    {
        await AddRestaurant("Sushi Train");
        await AddRestaurant("Sushi Hub");
        await AddRestaurant("Pizza Place");

        var results = await _restaurantService.SearchAsync("  sUsHi ", null, null, null);

        Assert.That(results.Select(r => r.Name), Is.EqualTo(new[] { "Sushi Hub", "Sushi Train" }));
    }

    [Test]
    public async Task Search_NoFilters_ReturnsAllSortedByName()
    {
        await AddRestaurant("Zeta");
        await AddRestaurant("Alpha");

        var results = await _restaurantService.SearchAsync(null, null, null, null);

        Assert.That(results.Select(r => r.Name), Is.EqualTo(new[] { "Alpha", "Zeta" }));
    }

    [Test]
    public async Task Search_NoMatches_ReturnsEmpty()
    {
        await AddRestaurant("Sushi Train");

        var results = await _restaurantService.SearchAsync("burger", null, null, null);

        Assert.That(results, Is.Empty);
    }

    [Test]
    public async Task Search_CuisineAndSuburb_FiltersBoth()
    {
        await AddRestaurant("Thai One", CuisineType.Thai, "Newtown");
        await AddRestaurant("Thai Two", CuisineType.Thai, "Parramatta");
        await AddRestaurant("Pho Newtown", CuisineType.Vietnamese, "Newtown");

        var results = await _restaurantService.SearchAsync(null, CuisineType.Thai, "newtown", null);

        Assert.That(results.Select(r => r.Name), Is.EqualTo(new[] { "Thai One" }));
    }

    [Test]
    public async Task Search_SortByPrice_ReturnsCheapestFirst()
    {
        await AddRestaurant("Fancy", priceLevel: 4);
        await AddRestaurant("Cheap", priceLevel: 1);

        var results = await _restaurantService.SearchAsync(null, null, null, "price");

        Assert.That(results.Select(r => r.Name), Is.EqualTo(new[] { "Cheap", "Fancy" }));
    }

    [Test]
    public async Task Search_SortByRating_ReturnsHighestRatedFirst()
    {
        var low = await AddRestaurant("Low");
        var high = await AddRestaurant("High");
        await AddRestaurant("Unrated");
        await AddReview(low.Id, 1, 2);
        await AddReview(high.Id, 1, 5);

        var results = await _restaurantService.SearchAsync(null, null, null, "rating");

        Assert.That(results.Select(r => r.Name), Is.EqualTo(new[] { "High", "Low", "Unrated" }));
    }

    [Test]
    public void Search_UnknownSort_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() =>
            _restaurantService.SearchAsync(null, null, null, "distance"));
    }

    [Test]
    public async Task GetRecommended_RanksByAverageThenReviewCount()
    {
        var oneFive = await AddRestaurant("One Five");
        var twoFives = await AddRestaurant("Two Fives");
        var three = await AddRestaurant("Three");
        await AddRestaurant("Unrated");
        await AddReview(oneFive.Id, 1, 5);
        await AddReview(twoFives.Id, 1, 5);
        await AddReview(twoFives.Id, 2, 5);
        await AddReview(three.Id, 1, 3);

        var results = await _restaurantService.GetRecommendedAsync(10);

        Assert.That(results.Select(r => r.Name), Is.EqualTo(new[] { "Two Fives", "One Five", "Three", "Unrated" }));
    }

    [Test]
    public async Task GetRecommended_LimitsToCount()
    {
        await AddRestaurant("A");
        await AddRestaurant("B");
        await AddRestaurant("C");

        var results = await _restaurantService.GetRecommendedAsync(2);

        Assert.That(results.Count(), Is.EqualTo(2));
    }

    [Test]
    public void GetRecommended_ZeroCount_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() => _restaurantService.GetRecommendedAsync(0));
    }

    [Test]
    public void GetById_MissingRestaurant_ThrowsNotFound()
    {
        Assert.ThrowsAsync<NotFoundException>(() => _restaurantService.GetByIdAsync(99));
    }
}

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
    private InMemoryRepository<Image> _images = null!;
    private InMemoryImageStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _restaurants = new InMemoryRepository<Restaurant>();
        _reviews = new InMemoryRepository<Review>();
        _images = new InMemoryRepository<Image>();
        _storage = new InMemoryImageStorage();
        _restaurantService = new RestaurantService(_restaurants, _reviews, new ImageService(_images, _storage));
    }

    private Task<Restaurant> AddRestaurant(string name, CuisineType cuisine = CuisineType.Other, string suburb = "Sydney", int priceLevel = 2, int ownerId = 0) =>
        _restaurants.AddAsync(new Restaurant
        {
            Name = name,
            Cuisine = cuisine,
            Address = "1 Test St",
            Suburb = suburb,
            PriceLevel = priceLevel,
            OwnerId = ownerId
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

        var results = await _restaurantService.SearchAsync("  sUsHi ", null, null, null, 1, 10);

        Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Sushi Hub", "Sushi Train" }));
    }

    [Test]
    public async Task Search_NoFilters_ReturnsAllSortedByName()
    {
        await AddRestaurant("Zeta");
        await AddRestaurant("Alpha");

        var results = await _restaurantService.SearchAsync(null, null, null, null, 1, 10);

        Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Alpha", "Zeta" }));
    }

    [Test]
    public async Task Search_NoMatches_ReturnsEmpty()
    {
        await AddRestaurant("Sushi Train");

        var results = await _restaurantService.SearchAsync("burger", null, null, null, 1, 10);

        Assert.That(results.Items, Is.Empty);
    }

    [Test]
    public async Task Search_CuisineAndSuburb_FiltersBoth()
    {
        await AddRestaurant("Thai One", CuisineType.Thai, "Newtown");
        await AddRestaurant("Thai Two", CuisineType.Thai, "Parramatta");
        await AddRestaurant("Pho Newtown", CuisineType.Vietnamese, "Newtown");

        var results = await _restaurantService.SearchAsync(null, CuisineType.Thai, "newtown", null, 1, 10);

        Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Thai One" }));
    }

    [Test]
    public async Task Search_SortByPrice_ReturnsCheapestFirst()
    {
        await AddRestaurant("Fancy", priceLevel: 4);
        await AddRestaurant("Cheap", priceLevel: 1);

        var results = await _restaurantService.SearchAsync(null, null, null, "price", 1, 10);

        Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Cheap", "Fancy" }));
    }

    [Test]
    public async Task Search_SortByRating_ReturnsHighestRatedFirst()
    {
        var low = await AddRestaurant("Low");
        var high = await AddRestaurant("High");
        await AddRestaurant("Unrated");
        await AddReview(low.Id, 1, 2);
        await AddReview(high.Id, 1, 5);

        var results = await _restaurantService.SearchAsync(null, null, null, "rating", 1, 10);

        Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "High", "Low", "Unrated" }));
    }

    [Test]
    public void Search_UnknownSort_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() =>
            _restaurantService.SearchAsync(null, null, null, "distance", 1, 10));
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

        var results = await _restaurantService.GetRecommendedAsync(1, 10);

        Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Two Fives", "One Five", "Three", "Unrated" }));
    }

    [Test]
    public async Task GetRecommended_SecondPage_ReturnsNextItemsAndTotals()
    {
        var first = await AddRestaurant("First");
        var second = await AddRestaurant("Second");
        await AddRestaurant("Third");
        await AddReview(first.Id, 1, 5);
        await AddReview(second.Id, 1, 4);

        var results = await _restaurantService.GetRecommendedAsync(2, 2);

        Assert.Multiple(() =>
        {
            Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Third" }));
            Assert.That(results.TotalCount, Is.EqualTo(3));
            Assert.That(results.TotalPages, Is.EqualTo(2));
        });
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(1, RestaurantService.MaxPageSize + 1)]
    public void GetRecommended_InvalidPaging_ThrowsBadRequest(int page, int pageSize)
    {
        Assert.ThrowsAsync<BadRequestException>(() => _restaurantService.GetRecommendedAsync(page, pageSize));
    }

    [Test]
    public async Task Search_PagesAfterFilteringAndSorting()
    {
        await AddRestaurant("Sushi C");
        await AddRestaurant("Pizza");
        await AddRestaurant("Sushi A");
        await AddRestaurant("Sushi B");

        var results = await _restaurantService.SearchAsync("sushi", null, null, null, 2, 2);

        Assert.Multiple(() =>
        {
            Assert.That(results.Items.Select(r => r.Name), Is.EqualTo(new[] { "Sushi C" }));
            Assert.That(results.Page, Is.EqualTo(2));
            Assert.That(results.TotalCount, Is.EqualTo(3));
            Assert.That(results.TotalPages, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Search_PagePastEnd_ReturnsEmptyItems()
    {
        await AddRestaurant("Only One");

        var results = await _restaurantService.SearchAsync(null, null, null, null, 5, 10);

        Assert.Multiple(() =>
        {
            Assert.That(results.Items, Is.Empty);
            Assert.That(results.TotalCount, Is.EqualTo(1));
        });
    }

    [Test]
    public void Search_InvalidPaging_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() =>
            _restaurantService.SearchAsync(null, null, null, null, 0, 10));
    }

    [Test]
    public void GetById_MissingRestaurant_ThrowsNotFound()
    {
        Assert.ThrowsAsync<NotFoundException>(() => _restaurantService.GetByIdAsync(99));
    }

    [Test]
    public void GetForOwner_NoRestaurantYet_ThrowsNotFound()
    {
        Assert.ThrowsAsync<NotFoundException>(() => _restaurantService.GetForOwnerAsync(7));
    }

    [Test]
    public async Task Create_SecondRestaurantForOwner_ThrowsConflict()
    {
        await AddRestaurant("First", ownerId: 7);

        Assert.ThrowsAsync<ConflictException>(() => _restaurantService.CreateAsync(new Restaurant
        {
            Name = "Second",
            Address = "2 Test St",
            Suburb = "Sydney",
            OwnerId = 7
        }));
    }

    [Test]
    public async Task Update_ChangesDetailsButNotOwner()
    {
        await AddRestaurant("Old Name", ownerId: 7);

        var updated = await _restaurantService.UpdateAsync(7, new Restaurant
        {
            Name = "New Name",
            Cuisine = CuisineType.Thai,
            Address = "9 New St",
            Suburb = "Newtown",
            PriceLevel = 3,
            Capacity = 50,
            OwnerId = 99
        });

        Assert.Multiple(() =>
        {
            Assert.That(updated.Name, Is.EqualTo("New Name"));
            Assert.That(updated.Cuisine, Is.EqualTo(CuisineType.Thai));
            Assert.That(updated.Capacity, Is.EqualTo(50));
            Assert.That(updated.OwnerId, Is.EqualTo(7));
        });
    }

    [Test]
    public async Task SetProfileImage_ReplacingImage_DeletesOldImageAndFile()
    {
        await AddRestaurant("Pic Place", ownerId: 7);

        var first = await _restaurantService.SetProfileImageAsync(7, TestImages.Png(), "first.png");
        int firstImageId = first.ProfileImageId!.Value;
        var second = await _restaurantService.SetProfileImageAsync(7, TestImages.Jpeg(), "second.jpg");

        var remaining = (await _images.GetAllAsync()).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(second.ProfileImageId, Is.Not.EqualTo(firstImageId));
            Assert.That(remaining.Select(i => i.Id), Is.EqualTo(new[] { second.ProfileImageId }));
            Assert.That(_storage.Files.Keys, Is.EqualTo(new[] { remaining[0].FileName }));
        });
    }

    [Test]
    public async Task RemoveProfileImage_ClearsLinkAndDeletesFile()
    {
        await AddRestaurant("Pic Place", ownerId: 7);
        await _restaurantService.SetProfileImageAsync(7, TestImages.Png(), "photo.png");

        var restaurant = await _restaurantService.RemoveProfileImageAsync(7);
        var remaining = await _images.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.That(restaurant.ProfileImageId, Is.Null);
            Assert.That(remaining, Is.Empty);
            Assert.That(_storage.Files, Is.Empty);
        });
    }

    [Test]
    public async Task SetProfileImage_InvalidFile_KeepsExistingImage()
    {
        await AddRestaurant("Pic Place", ownerId: 7);
        var withImage = await _restaurantService.SetProfileImageAsync(7, TestImages.Png(), "photo.png");
        int imageId = withImage.ProfileImageId!.Value;

        Assert.ThrowsAsync<BadRequestException>(() =>
            _restaurantService.SetProfileImageAsync(7, new MemoryStream("not an image"u8.ToArray()), "fake.png"));

        var restaurant = await _restaurantService.GetForOwnerAsync(7);
        Assert.Multiple(() =>
        {
            Assert.That(restaurant.ProfileImageId, Is.EqualTo(imageId));
            Assert.That(_storage.Files, Has.Count.EqualTo(1));
        });
    }
}

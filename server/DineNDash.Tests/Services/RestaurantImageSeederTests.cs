using DineNDash.Core.Models;
using DineNDash.Core.Services;
using DineNDash.Tests.Fakes;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers name matching, linking through the upload flow and repeat runs in RestaurantImageSeeder
/// </summary>
public class RestaurantImageSeederTests
{
    private RestaurantImageSeeder _seeder = null!;
    private RestaurantService _restaurantService = null!;
    private InMemoryRepository<Restaurant> _restaurants = null!;
    private InMemoryRepository<Image> _images = null!;
    private InMemoryImageStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _restaurants = new InMemoryRepository<Restaurant>();
        _images = new InMemoryRepository<Image>();
        _storage = new InMemoryImageStorage();
        _restaurantService = new RestaurantService(_restaurants, new InMemoryRepository<Review>(), new ImageService(_images, _storage));
        _seeder = new RestaurantImageSeeder(_restaurants, _restaurantService);
    }

    [Test]
    public async Task Seed_LinksEachPhotoToItsRestaurant()
    {
        await AddRestaurant("Chat Thai", ownerId: 1);
        await AddRestaurant("Mary's", ownerId: 2);

        var result = await _seeder.SeedAsync([Seed("Chat Thai", "chat-thai.jpg"), Seed("Mary's", "marys.jpg")]);

        var restaurants = (await _restaurants.GetAllAsync()).ToList();
        var images = (await _images.GetAllAsync()).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(result.Seeded, Is.EqualTo(2));
            Assert.That(restaurants.All(r => r.ProfileImageId != null), Is.True);
            Assert.That(images, Has.Count.EqualTo(2));
            Assert.That(images.Select(i => i.OriginalFileName), Is.EquivalentTo(new[] { "chat-thai.jpg", "marys.jpg" }));
            Assert.That(images.All(i => i.ContentType == "image/jpeg"), Is.True);
            Assert.That(_storage.Files, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public async Task Seed_SecondRun_AddsNothing()
    {
        await AddRestaurant("Chat Thai", ownerId: 1);
        var seeds = new[] { Seed("Chat Thai", "chat-thai.jpg") };

        await _seeder.SeedAsync(seeds);
        int imageId = (await _restaurants.GetAllAsync()).Single().ProfileImageId!.Value;
        var second = await _seeder.SeedAsync(seeds);

        Assert.Multiple(() =>
        {
            Assert.That(second.Seeded, Is.EqualTo(0));
            Assert.That(second.Skipped, Is.EqualTo(1));
            Assert.That(_restaurants.GetAllAsync().Result.Single().ProfileImageId, Is.EqualTo(imageId));
            Assert.That(_images.GetAllAsync().Result.Count(), Is.EqualTo(1));
            Assert.That(_storage.Files, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Seed_UnknownRestaurant_IsReportedNotCreated()
    {
        await AddRestaurant("Chat Thai", ownerId: 1);

        var result = await _seeder.SeedAsync([Seed("Nowhere Cafe", "nowhere.jpg")]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Unmatched, Is.EqualTo(new[] { "Nowhere Cafe" }));
            Assert.That(result.Seeded, Is.EqualTo(0));
            Assert.That(_restaurants.GetAllAsync().Result.Count(), Is.EqualTo(1));
            Assert.That(_images.GetAllAsync().Result, Is.Empty);
        });
    }

    [Test]
    public async Task Seed_SameNameTwice_IsReportedAndNeitherGetsTheImage()
    {
        await AddRestaurant("Chat Thai", ownerId: 1);
        await AddRestaurant("Chat Thai", ownerId: 2);

        var result = await _seeder.SeedAsync([Seed("Chat Thai", "chat-thai.jpg")]);

        var restaurants = (await _restaurants.GetAllAsync()).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(result.Ambiguous, Is.EqualTo(new[] { "Chat Thai" }));
            Assert.That(result.Seeded, Is.EqualTo(0));
            Assert.That(restaurants.All(r => r.ProfileImageId == null), Is.True);
            Assert.That(_images.GetAllAsync().Result, Is.Empty);
        });
    }

    [Test]
    public async Task Seed_KeepsImageTheOwnerUploaded()
    {
        await AddRestaurant("Chat Thai", ownerId: 1);
        var uploaded = await _restaurantService.SetProfileImageAsync(1, TestImages.Png(), "owner-photo.png");

        var result = await _seeder.SeedAsync([Seed("Chat Thai", "chat-thai.jpg")]);

        var restaurant = (await _restaurants.GetAllAsync()).Single();
        Assert.Multiple(() =>
        {
            Assert.That(result.Skipped, Is.EqualTo(1));
            Assert.That(restaurant.ProfileImageId, Is.EqualTo(uploaded.ProfileImageId));
            Assert.That(_storage.Files, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task Seed_MatchesNameIgnoringCaseAndSurroundingSpaces()
    {
        await AddRestaurant("Chat Thai", ownerId: 1);

        var result = await _seeder.SeedAsync([Seed("  chat thai ", "chat-thai.jpg")]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Seeded, Is.EqualTo(1));
            Assert.That(result.Unmatched, Is.Empty);
        });
    }

    private Task<Restaurant> AddRestaurant(string name, int ownerId) =>
        _restaurants.AddAsync(new Restaurant { Name = name, Address = "1 Test St", Suburb = "Sydney", OwnerId = ownerId });

    private static RestaurantSeedImage Seed(string restaurantName, string fileName) =>
        new(restaurantName, fileName, () => TestImages.Jpeg());
}

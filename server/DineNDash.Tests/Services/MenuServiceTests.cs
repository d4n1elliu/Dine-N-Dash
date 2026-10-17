using DineNDash.Core.Exceptions;
using DineNDash.Core.Models;
using DineNDash.Core.Services;
using DineNDash.Tests.Fakes;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers categories, items, ownership checks and image clean up in MenuService
/// </summary>
public class MenuServiceTests
{
    private const int OwnerId = 1;
    private const int OtherOwnerId = 2;

    private MenuService _menuService = null!;
    private InMemoryRepository<Restaurant> _restaurants = null!;
    private InMemoryRepository<MenuCategory> _categories = null!;
    private InMemoryRepository<MenuItem> _items = null!;
    private InMemoryRepository<Image> _images = null!;
    private InMemoryImageStorage _storage = null!;
    private Restaurant _restaurant = null!;

    [SetUp]
    public async Task SetUp()
    {
        _restaurants = new InMemoryRepository<Restaurant>();
        _categories = new InMemoryRepository<MenuCategory>();
        _items = new InMemoryRepository<MenuItem>();
        _images = new InMemoryRepository<Image>();
        _storage = new InMemoryImageStorage();

        var imageService = new ImageService(_images, _storage);
        var restaurantService = new RestaurantService(_restaurants, new InMemoryRepository<Review>(), imageService);
        _menuService = new MenuService(_categories, _items, restaurantService, imageService);

        _restaurant = await AddRestaurant("Mine", OwnerId);
        await AddRestaurant("Theirs", OtherOwnerId);
    }

    private Task<Restaurant> AddRestaurant(string name, int ownerId) =>
        _restaurants.AddAsync(new Restaurant { Name = name, Address = "1 Test St", Suburb = "Sydney", OwnerId = ownerId });

    private Task<MenuItem> AddItem(int categoryId, string name, decimal price = 10) =>
        _menuService.CreateItemAsync(OwnerId, new MenuItem { MenuCategoryId = categoryId, Name = name, Price = price });

    [Test]
    public async Task CreateCategory_TrimsNameAndLinksToOwnersRestaurant()
    {
        var category = await _menuService.CreateCategoryAsync(OwnerId, "  Mains ");

        Assert.Multiple(() =>
        {
            Assert.That(category.Name, Is.EqualTo("Mains"));
            Assert.That(category.RestaurantId, Is.EqualTo(_restaurant.Id));
        });
    }

    [Test]
    public async Task CreateCategory_DuplicateNameIgnoringCase_ThrowsConflict()
    {
        await _menuService.CreateCategoryAsync(OwnerId, "Mains");

        Assert.ThrowsAsync<ConflictException>(() => _menuService.CreateCategoryAsync(OwnerId, "MAINS"));
    }

    [Test]
    public async Task CreateCategory_SameNameAtAnotherRestaurant_IsAllowed()
    {
        await _menuService.CreateCategoryAsync(OtherOwnerId, "Mains");

        var category = await _menuService.CreateCategoryAsync(OwnerId, "Mains");

        Assert.That(category.Id, Is.GreaterThan(0));
    }

    [Test]
    public void CreateCategory_OwnerWithoutRestaurant_ThrowsNotFound()
    {
        Assert.ThrowsAsync<NotFoundException>(() => _menuService.CreateCategoryAsync(99, "Mains"));
    }

    [Test]
    public async Task RenameCategory_ToItsOwnNameWithNewCase_IsAllowed()
    {
        var category = await _menuService.CreateCategoryAsync(OwnerId, "mains");

        var renamed = await _menuService.RenameCategoryAsync(OwnerId, category.Id, "Mains");

        Assert.That(renamed.Name, Is.EqualTo("Mains"));
    }

    [Test]
    public async Task CreateItem_InAnotherOwnersCategory_ThrowsNotFound()
    {
        var theirCategory = await _menuService.CreateCategoryAsync(OtherOwnerId, "Drinks");

        Assert.ThrowsAsync<NotFoundException>(() => AddItem(theirCategory.Id, "Sneaky item"));
    }

    [Test]
    public async Task GetMenu_GroupsItemsUnderTheirCategories()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var drinks = await _menuService.CreateCategoryAsync(OwnerId, "Drinks");
        await AddItem(mains.Id, "Steak");
        await AddItem(drinks.Id, "Tea");
        await AddItem(mains.Id, "Pasta");

        var menu = (await _menuService.GetMenuAsync(_restaurant.Id)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(menu.Select(c => c.Name), Is.EqualTo(new[] { "Mains", "Drinks" }));
            Assert.That(menu[0].MenuItems.Select(i => i.Name), Is.EqualTo(new[] { "Pasta", "Steak" }));
            Assert.That(menu[1].MenuItems.Select(i => i.Name), Is.EqualTo(new[] { "Tea" }));
        });
    }

    [Test]
    public async Task UpdateItem_MovesToAnotherOwnCategory()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var specials = await _menuService.CreateCategoryAsync(OwnerId, "Specials");
        var item = await AddItem(mains.Id, "Steak");

        var updated = await _menuService.UpdateItemAsync(OwnerId, item.Id,
            new MenuItem { MenuCategoryId = specials.Id, Name = "Wagyu Steak", Price = 45 });

        Assert.Multiple(() =>
        {
            Assert.That(updated.MenuCategoryId, Is.EqualTo(specials.Id));
            Assert.That(updated.Name, Is.EqualTo("Wagyu Steak"));
            Assert.That(updated.Price, Is.EqualTo(45));
        });
    }

    [Test]
    public async Task UpdateItem_MoveToAnotherOwnersCategory_ThrowsNotFound()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var theirCategory = await _menuService.CreateCategoryAsync(OtherOwnerId, "Drinks");
        var item = await AddItem(mains.Id, "Steak");

        Assert.ThrowsAsync<NotFoundException>(() => _menuService.UpdateItemAsync(OwnerId, item.Id,
            new MenuItem { MenuCategoryId = theirCategory.Id, Name = "Steak", Price = 10 }));
    }

    [Test]
    public async Task DeleteItem_ByAnotherOwner_ThrowsNotFound()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var item = await AddItem(mains.Id, "Steak");

        Assert.ThrowsAsync<NotFoundException>(() => _menuService.DeleteItemAsync(OtherOwnerId, item.Id));
    }

    [Test]
    public async Task SetItemImage_ReplacingImage_DeletesOldFile()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var item = await AddItem(mains.Id, "Steak");

        await _menuService.SetItemImageAsync(OwnerId, item.Id, TestImages.Png(), "old.png");
        var updated = await _menuService.SetItemImageAsync(OwnerId, item.Id, TestImages.Jpeg(), "new.jpg");

        var remaining = (await _images.GetAllAsync()).ToList();
        Assert.Multiple(() =>
        {
            Assert.That(remaining.Select(i => i.Id), Is.EqualTo(new[] { updated.ImageId }));
            Assert.That(_storage.Files.Keys, Is.EqualTo(new[] { remaining[0].FileName }));
        });
    }

    [Test]
    public async Task DeleteItem_DeletesItsImage()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var item = await AddItem(mains.Id, "Steak");
        await _menuService.SetItemImageAsync(OwnerId, item.Id, TestImages.Png(), "steak.png");

        await _menuService.DeleteItemAsync(OwnerId, item.Id);

        var images = await _images.GetAllAsync();
        Assert.Multiple(() =>
        {
            Assert.That(images, Is.Empty);
            Assert.That(_storage.Files, Is.Empty);
        });
    }

    [Test]
    public async Task DeleteCategory_DeletesItsItemsAndTheirImages()
    {
        var mains = await _menuService.CreateCategoryAsync(OwnerId, "Mains");
        var drinks = await _menuService.CreateCategoryAsync(OwnerId, "Drinks");
        var steak = await AddItem(mains.Id, "Steak");
        await AddItem(drinks.Id, "Tea");
        await _menuService.SetItemImageAsync(OwnerId, steak.Id, TestImages.Png(), "steak.png");

        await _menuService.DeleteCategoryAsync(OwnerId, mains.Id);

        var menu = (await _menuService.GetMenuAsync(_restaurant.Id)).ToList();
        var items = await _items.GetAllAsync();
        Assert.Multiple(() =>
        {
            Assert.That(menu.Select(c => c.Name), Is.EqualTo(new[] { "Drinks" }));
            Assert.That(items.Select(i => i.Name), Is.EqualTo(new[] { "Tea" }));
            Assert.That(_storage.Files, Is.Empty);
        });
    }
}

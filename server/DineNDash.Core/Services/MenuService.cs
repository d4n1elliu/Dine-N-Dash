using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;
/// <summary>
/// Menu rules: unique category names per restaurant, items kept in the owner's own categories, and image clean up
/// </summary>
public class MenuService : IMenuService
{
    private IRepository<MenuCategory> _categories;
    private IRepository<MenuItem> _items;
    private IRestaurantService _restaurants;
    private IImageService _images;

    public MenuService(IRepository<MenuCategory> categories, IRepository<MenuItem> items, IRestaurantService restaurants, IImageService images)
    {
        _categories = categories;
        _items = items;
        _restaurants = restaurants;
        _images = images;
    }

    /// <summary>
    /// Returns the restaurant's categories with their items sorted by name
    /// </summary>
    public async Task<IEnumerable<MenuCategory>> GetMenuAsync(int restaurantId)
    {
        await _restaurants.GetByIdAsync(restaurantId);

        var allCategories = await _categories.GetAllAsync();
        var allItems = await _items.GetAllAsync();
        var categories = allCategories.Where(category => category.RestaurantId == restaurantId)
                                      .OrderBy(category => category.Id)
                                      .ToList();

        foreach (var category in categories)
        {
            category.MenuItems = allItems.Where(item => item.MenuCategoryId == category.Id)
                                         .OrderBy(item => item.Name)
                                         .ToList();
        }

        return categories;
    }

    /// <summary>
    /// Adds a category to the owner's restaurant
    /// </summary>
    public async Task<MenuCategory> CreateCategoryAsync(int ownerId, string name)
    {
        var restaurant = await _restaurants.GetForOwnerAsync(ownerId);
        var category = new MenuCategory { Name = (name ?? string.Empty).Trim(), RestaurantId = restaurant.Id };
        category.Validate();
        await EnsureNameIsFreeAsync(category);

        return await _categories.AddAsync(category);
    }

    /// <summary>
    /// Renames one of the owner's categories
    /// </summary>
    public async Task<MenuCategory> RenameCategoryAsync(int ownerId, int categoryId, string name)
    {
        var category = await GetOwnedCategoryAsync(ownerId, categoryId);
        category.Name = (name ?? string.Empty).Trim();
        category.Validate();
        await EnsureNameIsFreeAsync(category);
        category.UpdatedAt = DateTime.UtcNow;

        await _categories.UpdateAsync(category);
        return category;
    }

    /// <summary>
    /// Deletes the item images first, since deleting the category removes the items that point at them
    /// </summary>
    public async Task DeleteCategoryAsync(int ownerId, int categoryId)
    {
        var category = await GetOwnedCategoryAsync(ownerId, categoryId);
        var allItems = await _items.GetAllAsync();

        foreach (var item in allItems.Where(item => item.MenuCategoryId == category.Id))
        {
            await _items.DeleteAsync(item.Id);
            if (item.ImageId != null)
            {
                await _images.DeleteAsync(item.ImageId.Value);
            }
        }

        await _categories.DeleteAsync(category.Id);
    }

    /// <summary>
    /// Adds an item to one of the owner's categories
    /// </summary>
    public async Task<MenuItem> CreateItemAsync(int ownerId, MenuItem item)
    {
        await GetOwnedCategoryAsync(ownerId, item.MenuCategoryId);
        item.Name = item.Name.Trim();
        item.Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim();
        item.ImageId = null;
        item.Validate();

        return await _items.AddAsync(item);
    }

    /// <summary>
    /// Saves new details on one of the owner's items; the image is changed separately
    /// </summary>
    public async Task<MenuItem> UpdateItemAsync(int ownerId, int itemId, MenuItem changes)
    {
        var item = await GetOwnedItemAsync(ownerId, itemId);
        if (changes.MenuCategoryId != item.MenuCategoryId)
        {
            await GetOwnedCategoryAsync(ownerId, changes.MenuCategoryId);
        }

        item.Name = changes.Name.Trim();
        item.Description = string.IsNullOrWhiteSpace(changes.Description) ? null : changes.Description.Trim();
        item.Price = changes.Price;
        item.MenuCategoryId = changes.MenuCategoryId;
        item.Validate();
        item.UpdatedAt = DateTime.UtcNow;

        await _items.UpdateAsync(item);
        return item;
    }

    /// <summary>
    /// Deletes the item and its image
    /// </summary>
    public async Task DeleteItemAsync(int ownerId, int itemId)
    {
        var item = await GetOwnedItemAsync(ownerId, itemId);
        await _items.DeleteAsync(item.Id);

        if (item.ImageId != null)
        {
            await _images.DeleteAsync(item.ImageId.Value);
        }
    }

    /// <summary>
    /// Points the item at the new image before deleting the old one
    /// </summary>
    public async Task<MenuItem> SetItemImageAsync(int ownerId, int itemId, Stream content, string fileName)
    {
        var item = await GetOwnedItemAsync(ownerId, itemId);
        var image = await _images.UploadAsync(content, fileName);
        int? oldImageId = item.ImageId;

        item.ImageId = image.Id;
        item.UpdatedAt = DateTime.UtcNow;
        await _items.UpdateAsync(item);

        if (oldImageId != null)
        {
            await _images.DeleteAsync(oldImageId.Value);
        }

        return item;
    }

    /// <summary>
    /// Clears the item's image and deletes its file
    /// </summary>
    public async Task<MenuItem> RemoveItemImageAsync(int ownerId, int itemId)
    {
        var item = await GetOwnedItemAsync(ownerId, itemId);
        if (item.ImageId == null)
        {
            return item;
        }

        int oldImageId = item.ImageId.Value;
        item.ImageId = null;
        item.UpdatedAt = DateTime.UtcNow;
        await _items.UpdateAsync(item);
        await _images.DeleteAsync(oldImageId);

        return item;
    }

    /// <summary>
    /// Treats another restaurant's category as missing so owners cannot probe each other's menus
    /// </summary>
    private async Task<MenuCategory> GetOwnedCategoryAsync(int ownerId, int categoryId)
    {
        var restaurant = await _restaurants.GetForOwnerAsync(ownerId);
        var category = await _categories.GetByIdAsync(categoryId);

        if (category == null || category.RestaurantId != restaurant.Id)
        {
            throw new NotFoundException("Unable to find menu category.");
        }

        return category;
    }

    private async Task<MenuItem> GetOwnedItemAsync(int ownerId, int itemId)
    {
        var item = await _items.GetByIdAsync(itemId) ?? throw new NotFoundException("Unable to find menu item.");

        try
        {
            await GetOwnedCategoryAsync(ownerId, item.MenuCategoryId);
        }
        catch (NotFoundException)
        {
            throw new NotFoundException("Unable to find menu item.");
        }

        return item;
    }

    private async Task EnsureNameIsFreeAsync(MenuCategory category)
    {
        var allCategories = await _categories.GetAllAsync();
        bool taken = allCategories.Any(existing => existing.RestaurantId == category.RestaurantId
                                                && existing.Id != category.Id
                                                && string.Equals(existing.Name, category.Name, StringComparison.OrdinalIgnoreCase));

        if (taken)
        {
            throw new ConflictException("Your menu already has a category with that name.");
        }
    }
}

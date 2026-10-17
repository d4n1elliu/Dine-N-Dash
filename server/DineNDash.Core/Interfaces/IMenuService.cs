using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Menu categories and the items in them; every change is limited to the owner's own restaurant
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// Returns the categories in the order they were created, each with its items filled in
    /// </summary>
    Task<IEnumerable<MenuCategory>> GetMenuAsync(int restaurantId);
    /// <summary>
    /// Throws ConflictException when the restaurant already has a category with that name, ignoring case
    /// </summary>
    Task<MenuCategory> CreateCategoryAsync(int ownerId, string name);
    Task<MenuCategory> RenameCategoryAsync(int ownerId, int categoryId, string name);
    /// <summary>
    /// Also deletes the category's items and their images
    /// </summary>
    Task DeleteCategoryAsync(int ownerId, int categoryId);
    /// <summary>
    /// Throws NotFoundException when the item's category is not in the owner's restaurant
    /// </summary>
    Task<MenuItem> CreateItemAsync(int ownerId, MenuItem item);
    /// <summary>
    /// Can also move the item to another of the owner's categories
    /// </summary>
    Task<MenuItem> UpdateItemAsync(int ownerId, int itemId, MenuItem changes);
    Task DeleteItemAsync(int ownerId, int itemId);
    /// <summary>
    /// Uploads a new image for the item and deletes the one it replaces
    /// </summary>
    Task<MenuItem> SetItemImageAsync(int ownerId, int itemId, Stream content, string fileName);
    Task<MenuItem> RemoveItemImageAsync(int ownerId, int itemId);
}

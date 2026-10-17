using DineNDash.Api.DTOs;
using DineNDash.Api.Extensions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Shows restaurant menus and lets owners manage their categories, items and item images
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private IMenuService _menuService;

    public MenuController(IMenuService menuService)
    {
        _menuService = menuService;
    }

    /// <summary>
    /// Lists a restaurant's categories with their items
    /// </summary>
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetMenu(int restaurantId)
    {
        var categories = await _menuService.GetMenuAsync(restaurantId);
        return Ok(categories.Select(ToResponse));
    }

    /// <summary>
    /// Adds a category to the owner's menu
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory(MenuCategoryRequest request)
    {
        var category = await _menuService.CreateCategoryAsync(User.GetUserId(), request.Name);
        return StatusCode(StatusCodes.Status201Created, ToResponse(category));
    }

    /// <summary>
    /// Renames one of the owner's categories
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> RenameCategory(int id, MenuCategoryRequest request)
    {
        var category = await _menuService.RenameCategoryAsync(User.GetUserId(), id, request.Name);
        return Ok(ToResponse(category));
    }

    /// <summary>
    /// Deletes one of the owner's categories along with its items
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await _menuService.DeleteCategoryAsync(User.GetUserId(), id);
        return NoContent();
    }

    /// <summary>
    /// Adds an item to one of the owner's categories
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem(MenuItemRequest request)
    {
        var item = await _menuService.CreateItemAsync(User.GetUserId(), ToModel(request));
        return StatusCode(StatusCodes.Status201Created, ToResponse(item));
    }

    /// <summary>
    /// Edits one of the owner's items, including moving it to another category
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("items/{id}")]
    public async Task<IActionResult> UpdateItem(int id, MenuItemRequest request)
    {
        var item = await _menuService.UpdateItemAsync(User.GetUserId(), id, ToModel(request));
        return Ok(ToResponse(item));
    }

    /// <summary>
    /// Deletes one of the owner's items and its image
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpDelete("items/{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        await _menuService.DeleteItemAsync(User.GetUserId(), id);
        return NoContent();
    }

    /// <summary>
    /// Uploads an item image as multipart form data in a field named file, replacing any existing one
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("items/{id}/image")]
    [RequestSizeLimit(ImageController.MaxUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageController.MaxUploadRequestBytes)]
    public async Task<IActionResult> UploadItemImage(int id, IFormFile file)
    {
        await using var content = file.OpenReadStream();
        var item = await _menuService.SetItemImageAsync(User.GetUserId(), id, content, file.FileName);
        return Ok(ToResponse(item));
    }

    /// <summary>
    /// Removes an item's image
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpDelete("items/{id}/image")]
    public async Task<IActionResult> RemoveItemImage(int id)
    {
        var item = await _menuService.RemoveItemImageAsync(User.GetUserId(), id);
        return Ok(ToResponse(item));
    }

    private static MenuItem ToModel(MenuItemRequest request) => new()
    {
        MenuCategoryId = request.MenuCategoryId,
        Name = request.Name,
        Description = request.Description,
        Price = request.Price
    };

    private static MenuCategoryResponse ToResponse(MenuCategory category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Items = category.MenuItems.Select(ToResponse)
    };

    private static MenuItemResponse ToResponse(MenuItem item) => new()
    {
        Id = item.Id,
        MenuCategoryId = item.MenuCategoryId,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        ImageUrl = ImageController.UrlFor(item.ImageId)
    };
}

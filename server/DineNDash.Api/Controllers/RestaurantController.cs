using DineNDash.Api.DTOs;
using DineNDash.Api.Extensions;
using DineNDash.Core.Enums;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Lets diners browse recommended restaurants, search them and view one, and lets owners manage their own profile
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private IRestaurantService _restaurantService;

    public RestaurantController(IRestaurantService restaurantService)
    {
        _restaurantService = restaurantService;
    }

    /// <summary>
    /// Lists a page of restaurants whose name partially matches, with optional cuisine, suburb and sort (name, price or rating)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string? name, CuisineType? cuisine, string? suburb, string? sortBy, int page = 1, int pageSize = 10)
    {
        var results = await _restaurantService.SearchAsync(name, cuisine, suburb, sortBy, page, pageSize);
        return Ok(ToPagedResponse(results));
    }

    /// <summary>
    /// Lists a page of the top rated restaurants for the main page
    /// </summary>
    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended(int page = 1, int pageSize = 10)
    {
        var results = await _restaurantService.GetRecommendedAsync(page, pageSize);
        return Ok(ToPagedResponse(results));
    }

    /// <summary>
    /// Returns one restaurant by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var restaurant = await _restaurantService.GetByIdAsync(id);
        return Ok(ToResponse(restaurant));
    }

    /// <summary>
    /// Returns the owner's restaurant, or 404 before they have created it
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var restaurant = await _restaurantService.GetForOwnerAsync(User.GetUserId());
        return Ok(ToProfileResponse(restaurant));
    }

    /// <summary>
    /// Creates the owner's restaurant profile; each owner has one
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPost("mine")]
    public async Task<IActionResult> CreateMine(RestaurantProfileRequest request)
    {
        var restaurant = ToModel(request);
        restaurant.OwnerId = User.GetUserId();

        var created = await _restaurantService.CreateAsync(restaurant);
        return StatusCode(StatusCodes.Status201Created, ToProfileResponse(created));
    }

    /// <summary>
    /// Edits the owner's restaurant details
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("mine")]
    public async Task<IActionResult> UpdateMine(RestaurantProfileRequest request)
    {
        var updated = await _restaurantService.UpdateAsync(User.GetUserId(), ToModel(request));
        return Ok(ToProfileResponse(updated));
    }

    /// <summary>
    /// Uploads a profile image as multipart form data in a field named file, replacing and deleting any existing one
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("mine/image")]
    [RequestSizeLimit(ImageController.MaxUploadRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = ImageController.MaxUploadRequestBytes)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        await using var content = file.OpenReadStream();
        var restaurant = await _restaurantService.SetProfileImageAsync(User.GetUserId(), content, file.FileName);
        return Ok(ToProfileResponse(restaurant));
    }

    /// <summary>
    /// Removes the owner's profile image
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpDelete("mine/image")]
    public async Task<IActionResult> RemoveImage()
    {
        var restaurant = await _restaurantService.RemoveProfileImageAsync(User.GetUserId());
        return Ok(ToProfileResponse(restaurant));
    }

    private static Restaurant ToModel(RestaurantProfileRequest request) => new()
    {
        Name = request.Name.Trim(),
        Description = request.Description.Trim(),
        Cuisine = request.Cuisine,
        Address = request.Address.Trim(),
        Suburb = request.Suburb.Trim(),
        PriceLevel = request.PriceLevel,
        Capacity = request.Capacity
    };

    private static RestaurantProfileResponse ToProfileResponse(Restaurant restaurant) => new()
    {
        Id = restaurant.Id,
        Name = restaurant.Name,
        Description = restaurant.Description,
        Cuisine = restaurant.Cuisine,
        Address = restaurant.Address,
        Suburb = restaurant.Suburb,
        PriceLevel = restaurant.PriceLevel,
        ProfileImageUrl = ImageController.UrlFor(restaurant.ProfileImageId),
        Capacity = restaurant.Capacity
    };

    private static PagedResponse<RestaurantResponse> ToPagedResponse(PagedResult<Restaurant> results) => new()
    {
        Items = results.Items.Select(ToResponse),
        Page = results.Page,
        PageSize = results.PageSize,
        TotalCount = results.TotalCount,
        TotalPages = results.TotalPages
    };

    private static RestaurantResponse ToResponse(Restaurant restaurant) => new()
    {
        Id = restaurant.Id,
        Name = restaurant.Name,
        Description = restaurant.Description,
        Cuisine = restaurant.Cuisine,
        Address = restaurant.Address,
        Suburb = restaurant.Suburb,
        PriceLevel = restaurant.PriceLevel,
        ProfileImageUrl = ImageController.UrlFor(restaurant.ProfileImageId)
    };
}

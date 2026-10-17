using DineNDash.Api.DTOs;
using DineNDash.Core.Enums;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Lets diners browse recommended restaurants, search them and view one
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
    /// Lists restaurants whose name partially matches, with optional cuisine, suburb and sort (name, price or rating)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Search(string? name, CuisineType? cuisine, string? suburb, string? sortBy)
    {
        var restaurants = await _restaurantService.SearchAsync(name, cuisine, suburb, sortBy);
        return Ok(restaurants.Select(ToResponse));
    }

    /// <summary>
    /// Lists the top rated restaurants for the main page
    /// </summary>
    [HttpGet("recommended")]
    public async Task<IActionResult> GetRecommended(int count = 10)
    {
        var restaurants = await _restaurantService.GetRecommendedAsync(count);
        return Ok(restaurants.Select(ToResponse));
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

    private static RestaurantResponse ToResponse(Restaurant restaurant) => new()
    {
        Id = restaurant.Id,
        Name = restaurant.Name,
        Description = restaurant.Description,
        Cuisine = restaurant.Cuisine,
        Address = restaurant.Address,
        Suburb = restaurant.Suburb,
        PriceLevel = restaurant.PriceLevel,
        PhotoUrls = restaurant.PhotoUrls
    };
}

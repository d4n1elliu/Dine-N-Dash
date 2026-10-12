using DineNDash.Api.DTOs;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase 
{
    private IReviewService _reviewService; 

    public ReviewController(IReviewService reviewService) 
    {
        _reviewService = reviewService; 
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int dinerId, CreateReviewRequest request)
    {
        var review = new Review
        {
            DinerId = dinerId,
            RestaurantId = request.RestaurantId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        var created = await _reviewService.CreateAsync(review);
        return StatusCode(StatusCodes.Status201Created, ToResponse(created));
    }

    [HttpPut("{id}")]
    public async Task <IActionResult> Update(int id, [FromQuery] int dinerId, UpdateReviewRequest request)
    {
        var review = new Review 
        {
            Id = id, 
            Rating = request.Rating,
            Comment = request.Comment
        };

        var updated = await _reviewService.UpdateAsync(review, dinerId);
        return Ok(ToResponse(updated));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, [FromQuery] int dinerId) 
    {
        await _reviewService.DeleteAsync(id, dinerId);
        return NoContent();
    }

    [HttpGet("diner/{dinerId}")]
    public async Task<IActionResult> GetForDiner(int dinerId) 
    {
        var reviews = await _reviewService.GetForDinerAsync(dinerId);
        return Ok(reviews.Select(ToResponse));
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetForRestaurant(int restaurantId) 
    {
        var reviews = await _reviewService.GetForRestaurantAsync(restaurantId);
        return Ok(reviews.Select(ToResponse));
    }

    [HttpGet("restaurant/{restaurantId}/average")]
    public async Task<IActionResult> GetAverage(int restaurantId) 
    {
        double average = await _reviewService.GetAverageRatingAsync(restaurantId);
        return Ok(new { average });
    }

    private static ReviewResponse ToResponse(Review review) => new() 
    {
        Id = review.Id,
        RestaurantId = review.RestaurantId,
        RestaurantName = review.Restaurant?.Name, 
        DinerName = review.Diner?.Name,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}

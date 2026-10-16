using DineNDash.Api.DTOs;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Mvc;
using DineNDash.Api.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Creates, edits, deletes and lists restaurant reviews
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase 
{
    private IReviewService _reviewService; 

    public ReviewController(IReviewService reviewService) 
    {
        _reviewService = reviewService; 
    }

    /// <summary>
    /// Adds a review from a diner for a restaurant
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateReviewRequest request)
    {
        var review = new Review
        {
            DinerId = User.GetUserId(),
            RestaurantId = request.RestaurantId,
            Rating = request.Rating,
            Comment = request.Comment
        };

        var created = await _reviewService.CreateAsync(review);
        return StatusCode(StatusCodes.Status201Created, ToResponse(created));
    }

    /// <summary>
    /// Edits the rating and comment of a review the diner wrote
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    public async Task <IActionResult> Update(int id, UpdateReviewRequest request)
    {
        var review = new Review 
        {
            Id = id, 
            Rating = request.Rating,
            Comment = request.Comment
        };

        var updated = await _reviewService.UpdateAsync(review, User.GetUserId());
        return Ok(ToResponse(updated));
    }

    /// <summary>
    /// Deletes a review the diner wrote
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id) 
    {
        await _reviewService.DeleteAsync(id, User.GetUserId());
        return NoContent();
    }

    /// <summary>
    /// Lists a diner's reviews, newest first
    /// </summary>
    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetForDiner() 
    {
        var reviews = await _reviewService.GetForDinerAsync(User.GetUserId());
        return Ok(reviews.Select(ToResponse));
    }

    /// <summary>
    /// Lists a restaurant's reviews, newest first
    /// </summary>
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetForRestaurant(int restaurantId) 
    {
        var reviews = await _reviewService.GetForRestaurantAsync(restaurantId);
        return Ok(reviews.Select(ToResponse));
    }
    
    /// <summary>
    /// Returns a restaurant's average rating, or 0 with no reviews
    /// </summary>
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

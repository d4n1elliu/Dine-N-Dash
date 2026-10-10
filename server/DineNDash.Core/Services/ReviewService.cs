using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;
public class ReviewService : IReviewService 
{
    private IRepository<Review> _reviews;
    private IRepository<Restaurant> _restaurants; 

    public ReviewService(IRepository<Review> reviews, IRepository<Restaurant> restaurants) 
    {
        _reviews = reviews;
        _restaurants = restaurants; 
    }

    public async Task<Review> CreateAsync(Review review) 
    { 
        review.Validate(); 

        var restaurant = await _restaurants.GetByIdAsync(review.RestaurantId); 
        if (restaurant == null) 
        {
            throw new NotFoundException("Unable to find restaurant.");
        }

        var allReviews = await _reviews.GetAllAsync();
        bool reviewed = allReviews.Any(r => r.DinerId == review.DinerId
                                         && r.RestaurantId == review.RestaurantId);
        
        if (reviewed) 
        {
            throw new ConflictException("You cannot review the same restaurant more than once."); 
        }

        return await _reviews.AddAsync(review); 
    }

    public async Task<Review> UpdateAsync(Review review, int dinerId) 
    { 
        var existing = await _reviews.GetByIdAsync(review.Id) ?? throw new NotFoundException("Unable to find review.");

        if (existing.DinerId != dinerId) 
        {
            throw new BadRequestException("You can only edit your own reviews.");
        }

        existing.Rating = review.Rating;
        existing.Comment = review.Comment; 
        existing.Validate();
        existing.UpdatedAt = DateTime.UtcNow;

        await _reviews.UpdateAsync(existing);
        return existing;
    }

    public async Task<IEnumerable<Review>> GetForDinerAsync(int dinerId) 
    { 
        var allReviews = await _reviews.GetAllAsync();
        return allReviews.Where(review => review.DinerId == dinerId)
                         .OrderByDescending(review => review.CreatedAt)
                         .ToList();
    }

    public async Task<IEnumerable<Review>> GetForRestaurantAsync(int restaurantId) 
    { 
        var allReviews = await _reviews.GetAllAsync();
        return allReviews.Where(review => review.RestaurantId == restaurantId)
                         .OrderByDescending(review => review.CreatedAt)
                         .ToList();
    }

    public async Task<double> GetAverageRatingAsync(int restaurantId) 
    { 
        var allReviews = await _reviews.GetAllAsync();
        var ratings = allReviews.Where(review => review.RestaurantId == restaurantId)
                                .Select(review => review.Rating)
                                .ToList();

        if (ratings.Count == 0) 
        {
            return 0; 
        }

        return ratings.Average(); 
    }

    public async Task DeleteAsync(int reviewId, int dinerId) 
    {
        var review = await _reviews.GetByIdAsync(reviewId) ?? throw new NotFoundException("Unable to find review."); 

        if (review.DinerId != dinerId) 
        {
            throw new BadRequestException("You can only delete your own reviews!"); 
        }

        await _reviews.DeleteAsync(reviewId); 
    }
}

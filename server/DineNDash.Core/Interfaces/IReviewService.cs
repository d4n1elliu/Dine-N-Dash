using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IReviewService
{
    /// <summary>
    /// Throws ConflictException when the diner has already reviewed the restaurant
    /// </summary>
    Task<Review> CreateAsync(Review review);
    Task<Review> UpdateAsync(Review review, int dinerId);
    Task<IEnumerable<Review>> GetForDinerAsync(int dinerId); 
    Task<IEnumerable<Review>> GetForRestaurantAsync(int restaurantId);
    /// <summary>
    /// Returns 0 rather than throwing when the restaurant has no reviews
    /// </summary>
    Task<double> GetAverageRatingAsync(int restaurantId);
    Task DeleteAsync(int reviewId, int dinerId);
}
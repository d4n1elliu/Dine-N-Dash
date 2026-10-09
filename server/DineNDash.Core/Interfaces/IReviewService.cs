using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IReviewService
{
    Task<Review> CreateAsync(Review review);
    Task<IEnumerable<Review>> GetForRestaurantAsync(int restaurantId);
    Task<double> GetAverageRatingAsync(int restaurantId);
    Task DeleteAsync(int reviewId, int dinerId);
}
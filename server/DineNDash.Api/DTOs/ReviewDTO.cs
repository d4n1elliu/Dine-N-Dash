namespace DineNDash.Api.DTOs; 
public class CreateReviewRequest 
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty; 
    public int RestaurantId { get; set; }
}
public class UpdateReviewRequest 
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty; 
}
public class ReviewResponse 
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public string? DinerName { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
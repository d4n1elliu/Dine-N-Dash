namespace DineNDash.Api.DTOs; 

// data from diner to make the review 
public class CreateReviewRequest 
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty; 
    public int RestaurantId { get; set; }
}

// data from diner to edit their review 
public class UpdateReviewRequest 
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty; 
}

// review data sent to frontend 
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
namespace DineNDash.Api.DTOs; 

/// <summary>
/// Details needed to post a review
/// </summary>
public class CreateReviewRequest 
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty; 
    public int RestaurantId { get; set; }
}

/// <summary>
/// Fields a diner may change on an existing review
/// </summary>
public class UpdateReviewRequest 
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty; 
}

/// <summary>
/// Review as shown to the client, with restaurant and diner names resolved
/// </summary>
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
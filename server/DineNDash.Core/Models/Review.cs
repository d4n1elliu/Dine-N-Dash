namespace DineNDash.Core.Models;
public class Review : BaseEntity
{
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public int DinerId { get; set; }
    public DinerAccount? Diner { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public override void Validate()
    {
        Require(Rating is >= 1 and <= 5, "Rating must be between 1 and 5.");
        Require(!string.IsNullOrWhiteSpace(Comment), "Comment is required.");
        Require(Comment.Length <= 500, "Comment must be 500 characters or fewer.");
    }
}
namespace DineNDash.Core.Models;

/// <summary>
/// A dish on a restaurant's menu with its price and category
/// </summary>
public class MenuItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public override void Validate()
    {
        Require(!string.IsNullOrWhiteSpace(Name), "Menu item name is required.");
        Require(Price >= 0, "Price cannot be negative.");
        Require(!string.IsNullOrWhiteSpace(Category), "Category is required.");
    }
}
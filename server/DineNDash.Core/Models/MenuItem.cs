namespace DineNDash.Core.Models;

/// <summary>
/// A dish on a restaurant's menu with its price, sitting in one of the restaurant's categories
/// </summary>
public class MenuItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int MenuCategoryId { get; set; }
    public MenuCategory? MenuCategory { get; set; }
    public int? ImageId { get; set; }
    public Image? Image { get; set; }
    public override void Validate()
    {
        Require(!string.IsNullOrWhiteSpace(Name), "Menu item name is required.");
        Require(Name.Length <= 100, "Menu item name must be 100 characters or fewer.");
        Require(Description is null || Description.Length <= 500, "Description must be 500 characters or fewer.");
        Require(Price >= 0, "Price cannot be negative.");
    }
}

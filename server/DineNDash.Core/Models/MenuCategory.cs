namespace DineNDash.Core.Models;

/// <summary>
/// A section of a restaurant's menu, such as Entrees or Drinks, that holds its menu items
/// </summary>
public class MenuCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public ICollection<MenuItem> MenuItems { get; set; } = [];
    public override void Validate()
    {
        Require(!string.IsNullOrWhiteSpace(Name), "Category name is required.");
        Require(Name.Length <= 50, "Category name must be 50 characters or fewer.");
    }
}

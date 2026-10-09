using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;
public class Restaurant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CuisineType Cuisine { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public int PriceLevel { get; set; } = 2;
    public int Capacity { get; set; } = 20;
    public ICollection<string> PhotoUrls { get; set; } = [];
    public int OwnerId { get; set; }
    public RestaurantAccount? Owner { get; set; }
    public ICollection<MenuItem> MenuItems { get; set; } = [];
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public override void Validate()
    {
        Require(!string.IsNullOrWhiteSpace(Name), "Restaurant name is required.");
        Require(Name.Length <= 100, "Restaurant name must be 100 characters or fewer.");
        Require(!string.IsNullOrWhiteSpace(Address), "Address is required.");
        Require(!string.IsNullOrWhiteSpace(Suburb), "Suburb is required.");
        Require(PriceLevel is >= 1 and <= 4, "Price level must be between 1 and 4.");
        Require(Capacity > 0, "Capacity must be greater than 0.");
    }
}
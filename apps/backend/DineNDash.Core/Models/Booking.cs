using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;
public class Booking : BaseEntity
{
    public DateTime BookingTime { get; set; }
    public int PartySize { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public string? SpecialRequests { get; set; }
    public int DinerId { get; set; }
    public DinerAccount? Diner { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public override void Validate()
    {
        Require(BookingTime > DateTime.UtcNow, "Booking time must be in the future.");
        Require(PartySize is >= 1 and <= 20, "Party size must be between 1 and 20.");
    }
}
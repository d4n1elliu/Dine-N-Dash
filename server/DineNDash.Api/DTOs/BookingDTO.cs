using DineNDash.Core.Enums;

namespace DineNDash.Api.DTOs; 

/// <summary>
/// Details needed to book a table
/// </summary>
public class CreateBookingRequest 
{
    public int RestaurantId { get; set; }
    public DateTimeOffset BookingTime { get; set; }
    public int PartySize { get; set; }
    public string? SpecialRequests { get; set; }
}

/// <summary>
/// Booking as shown to the client, with the restaurant and diner names resolved where loaded
/// </summary>
public class BookingResponse 
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public string? DinerName { get; set; }
    public DateTime BookingTime { get; set; }
    public int PartySize { get; set; }
    public BookingStatus Status { get; set; } 
    public string? SpecialRequests { get; set; }
}
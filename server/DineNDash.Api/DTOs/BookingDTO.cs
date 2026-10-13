using DineNDash.Core.Enums;

namespace DineNDash.Api.DTOs; 
public class CreateBookingRequest 
{
    public int RestaurantId { get; set; }
    public DateTimeOffset BookingTime { get; set; }
    public int PartySize { get; set; }
    public string? SpecialRequests { get; set; }
}
public class BookingResponse 
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string? RestaurantName { get; set; }
    public DateTime BookingTime { get; set; }
    public int PartySize { get; set; }
    public BookingStatus Status { get; set; } 
    public string? SpecialRequests { get; set; }
}
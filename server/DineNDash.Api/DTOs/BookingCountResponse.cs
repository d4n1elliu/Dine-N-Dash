namespace DineNDash.Api.DTOs;

/// <summary>
/// One bar of the owner's bookings chart
/// </summary>
public class BookingCountResponse
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public int Bookings { get; set; }
    public int Guests { get; set; }
}
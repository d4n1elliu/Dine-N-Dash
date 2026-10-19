namespace DineNDash.Core.Models;

/// <summary>
/// Bookings and guests counted for a run of days (a single day, or part of a week), both ends included
/// </summary>
public class BookingCountBucket
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public int Bookings { get; set; }
    public int Guests { get; set; }
}
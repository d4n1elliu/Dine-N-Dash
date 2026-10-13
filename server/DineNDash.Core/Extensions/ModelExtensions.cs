using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Extensions;
public static class ModelExtensions
{
    public static string ToDisplayTime(this Booking booking) =>
        booking.BookingTime.ToLocalTime().ToString("ddd d MMM yyyy, h:mm tt");
    public static string ToPriceSymbols(this Restaurant restaurant) =>
        new string('$', restaurant.PriceLevel);
    public static string ToDisplayName(this CuisineType cuisine) =>
        string.Concat(cuisine.ToString().Select(c => char.IsUpper(c) ? " " + c : c.ToString())).Trim();
    public static bool CanBeCancelled(this Booking booking) =>
        booking.Status is BookingStatus.Pending or BookingStatus.Confirmed
        && booking.BookingTime > DateTime.UtcNow;
}
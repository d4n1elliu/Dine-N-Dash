using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IBookingService
{
    Task<Booking> CreateAsync(Booking booking);
    Task<IEnumerable<Booking>> GetForDinerAsync(int dinerId);
    Task<IEnumerable<Booking>> GetForRestaurantAsync(int restaurantId);
    Task<Booking> UpdateStatusAsync(int bookingId, BookingStatus status);
    Task<bool> IsSlotAvailableAsync(int restaurantId, DateTime time, int partySize);
    Task CancelAsync(int bookingId, int dinerId);
}
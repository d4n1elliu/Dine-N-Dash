using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IBookingService
{
    /// <summary>
    /// Throws ConflictException when the slot cannot fit the party
    /// </summary>
    Task<Booking> CreateAsync(Booking booking);
    Task<IEnumerable<Booking>> GetForDinerAsync(int dinerId);
    Task<IEnumerable<Booking>> GetForRestaurantAsync(int restaurantId);
    /// <summary>
    /// Returns the bookings at the owner's restaurant with each diner filled in
    /// </summary>
    Task<IEnumerable<Booking>> GetForOwnerAsync(int ownerId);
    /// <summary>
    /// Confirms or declines a pending, upcoming booking at the owner's restaurant
    /// </summary>
    Task<Booking> RespondAsync(int ownerId, int bookingId, bool accept);
    Task<bool> IsSlotAvailableAsync(int restaurantId, DateTime time, int partySize);
    Task CancelAsync(int bookingId, int dinerId);
}
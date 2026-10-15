using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Extensions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;
/// <summary>
/// Booking rules: validation, capacity checks and cancellation
/// </summary>
public class BookingService : IBookingService 
{
    private IRepository<Booking> _bookings;
    private IRepository<Restaurant> _restaurants; 

    public BookingService(IRepository<Booking> bookings, IRepository<Restaurant> restaurants) 
    {
        _bookings = bookings;
        _restaurants = restaurants;
    }

    /// <summary>
    /// Validates the booking, rejects it when the slot is full and stores it as Pending
    /// </summary>
    public async Task<Booking> CreateAsync(Booking booking) 
    { 
        booking.Validate();

        if (!await IsSlotAvailableAsync(booking.RestaurantId, booking.BookingTime, booking.PartySize)) 
        {
            throw new ConflictException("No availabilities for that time slot!"); 
        }

        booking.Status = BookingStatus.Pending;
        return await _bookings.AddAsync(booking); 
    }

    /// <summary>
    /// Returns a diner's bookings ordered by time
    /// </summary>
    public async Task<IEnumerable<Booking>> GetForDinerAsync(int dinerId) 
    { 
        var allBookings = await _bookings.GetAllAsync();
        return allBookings.Where(booking => booking.DinerId == dinerId)
                          .OrderBy(booking => booking.BookingTime)
                          .ToList(); 
    }

    /// <summary>
    /// Returns a restaurant's bookings ordered by time
    /// </summary>
    public async Task<IEnumerable<Booking>> GetForRestaurantAsync(int restaurantId) 
    { 
        var allBookings = await _bookings.GetAllAsync();
        return allBookings.Where(booking => booking.RestaurantId == restaurantId)
                          .OrderBy(booking => booking.BookingTime)
                          .ToList(); 
    }

    /// <summary>
    /// Sets a new status on an existing booking, used by the restaurant side
    /// </summary>
    public async Task<Booking> UpdateStatusAsync(int bookingId, BookingStatus status) 
    { 
        var booking = await _bookings.GetByIdAsync(bookingId) ?? throw new NotFoundException("Unable to find booking."); 
        booking.Status = status;
        booking.UpdatedAt = DateTime.UtcNow; 
        await _bookings.UpdateAsync(booking); 
        return booking; 

    }

    /// <summary>
    /// Checks whether the party fits in the capacity left after pending and confirmed bookings at that exact time
    /// </summary>
    public async Task<bool> IsSlotAvailableAsync(int restaurantId, DateTime time, int partySize) 
    {
        var restaurant = await _restaurants.GetByIdAsync(restaurantId) ?? throw new NotFoundException("Unable to find restaurant.");
        var allBookings = await _bookings.GetAllAsync();
        int spotsTaken = allBookings.Where(booking => booking.RestaurantId == restaurantId
                                                   && booking.BookingTime == time
                                                   && (booking.Status == BookingStatus.Pending || booking.Status == BookingStatus.Confirmed))
                                    .Sum(booking => booking.PartySize); 

        return spotsTaken + partySize <= restaurant.Capacity; 
     }

    /// <summary>
    /// Cancels a booking only for the diner who made it and only while it is still upcoming
    /// </summary>
    public async Task CancelAsync(int bookingId, int dinerId) 
    { 
        var booking = await _bookings.GetByIdAsync(bookingId) ?? throw new NotFoundException("Unable to find booking."); 
        if (booking.DinerId != dinerId) {
            throw new BadRequestException("You can only cancel your own bookings!");
        }

        if (!booking.CanBeCancelled()) {
            throw new BadRequestException("Booking cannot be cancelled anymore!"); 
        }

        booking.Status = BookingStatus.Cancelled; 
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookings.UpdateAsync(booking); 
    }
}

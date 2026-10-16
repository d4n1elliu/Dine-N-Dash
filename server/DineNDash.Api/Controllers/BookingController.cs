using DineNDash.Api.DTOs;
using DineNDash.Core.Enums;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Mvc;
using DineNDash.Api.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Creates, lists, updates and cancels table bookings
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase 
{
    private IBookingService _bookingService; 

    public BookingController(IBookingService bookingService) 
    {
        _bookingService = bookingService; 
    }

    /// <summary>
    /// Books a table for a diner if the slot has room
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        var booking = new Booking 
        {
            DinerId = User.GetUserId(),
            RestaurantId = request.RestaurantId,
            BookingTime = request.BookingTime.UtcDateTime,
            PartySize = request.PartySize,
            SpecialRequests = request.SpecialRequests
        };

        var created = await _bookingService.CreateAsync(booking); 
        return StatusCode(StatusCodes.Status201Created, ToResponse(created));
    }

    /// <summary>
    /// Lists a diner's bookings, earliest first
    /// </summary>
    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine() 
    {
        var bookings = await _bookingService.GetForDinerAsync(User.GetUserId());
        return Ok(bookings.Select(ToResponse)); 
    }

    /// <summary>
    /// Lists a restaurant's bookings, earliest first
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetForRestaurant(int restaurantId) 
    {
        var bookings = await _bookingService.GetForRestaurantAsync(restaurantId); 
        return Ok(bookings.Select(ToResponse));
    }

    /// <summary>
    /// Reports whether a party fits at a restaurant at a given time
    /// </summary>
    [Authorize]
    [HttpGet("availability")]
    public async Task<IActionResult> CheckAvailability(int restaurantId, DateTimeOffset time, int partySize)
    {
        bool available = await _bookingService.IsSlotAvailableAsync(restaurantId, time.UtcDateTime, partySize); 
        return Ok(new { available }); 
    }

    /// <summary>
    /// Changes a booking status, for example when the restaurant confirms it
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpPut("{id}/status")]
    public async Task <IActionResult> UpdateStatus(int id, [FromQuery] BookingStatus status)
    {
        var updated = await _bookingService.UpdateStatusAsync(id, status); 
        return Ok(ToResponse(updated)); 
    }

    /// <summary>
    /// Cancels a booking on behalf of the diner who made it
    /// </summary>
    [Authorize]
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id) 
    {
        await _bookingService.CancelAsync(id, User.GetUserId());
        return NoContent(); 
    }

    private static BookingResponse ToResponse(Booking booking) => new() 
    {
        Id = booking.Id,
        RestaurantId = booking.RestaurantId,
        RestaurantName = booking.Restaurant?.Name,
        BookingTime = booking.BookingTime,
        PartySize = booking.PartySize,
        Status = booking.Status, 
        SpecialRequests = booking.SpecialRequests
    }; 
}
using DineNDash.Api.DTOs;
using DineNDash.Core.Enums;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase 
{
    private IBookingService _bookingService; 

    public BookingController(IBookingService bookingService) 
    {
        _bookingService = bookingService; 
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromQuery] int dinerId, CreateBookingRequest request)
    {
        var booking = new Booking 
        {
            DinerId = dinerId,
            RestaurantId = request.RestaurantId,
            BookingTime = request.BookingTime.UtcDateTime,
            PartySize = request.PartySize,
            SpecialRequests = request.SpecialRequests
        };

        var created = await _bookingService.CreateAsync(booking); 
        return StatusCode(StatusCodes.Status201Created, ToResponse(created));
    }

    [HttpGet("diner/{dinerId}")]
    public async Task<IActionResult> GetForDiner(int dinerId) 
    {
        var bookings = await _bookingService.GetForDinerAsync(dinerId);
        return Ok(bookings.Select(ToResponse)); 
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetForRestaurant(int restaurantId) 
    {
        var bookings = await _bookingService.GetForRestaurantAsync(restaurantId); 
        return Ok(bookings.Select(ToResponse));
    }

    [HttpGet("availability")]
    public async Task<IActionResult> CheckAvailability(int restaurantId, DateTimeOffset time, int partySize)
    {
        bool available = await _bookingService.IsSlotAvailableAsync(restaurantId, time.UtcDateTime, partySize); 
        return Ok(new { available }); 
    }

    [HttpPut("{id}/status")]
    public async Task <IActionResult> UpdateStatus(int id, [FromQuery] BookingStatus status)
    {
        var updated = await _bookingService.UpdateStatusAsync(id, status); 
        return Ok(ToResponse(updated)); 
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromQuery] int dinerId) 
    {
        await _bookingService.CancelAsync(id, dinerId);
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
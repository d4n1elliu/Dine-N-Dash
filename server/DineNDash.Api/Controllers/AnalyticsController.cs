using DineNDash.Api.DTOs;
using DineNDash.Api.Extensions;
using DineNDash.Core.Enums;
using DineNDash.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Booking statistics for a restaurant owner's dashboard
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IBookingAnalyticsService _analyticsService;

    public AnalyticsController(IBookingAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Counts the owner's bookings per day or per week, for the chart
    /// </summary>
    [Authorize(Roles = "RestaurantOwner")]
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookingCounts([FromQuery] AnalyticsPeriod period = AnalyticsPeriod.Day)
    {
        var buckets = await _analyticsService.GetBookingCountsAsync(User.GetUserId(), period);
        return Ok(buckets.Select(b => new BookingCountResponse
        {
            Start = b.Start,
            End = b.End,
            Bookings = b.Bookings,
            Guests = b.Guests
        }));
    }
}
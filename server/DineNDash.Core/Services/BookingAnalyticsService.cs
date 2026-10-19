using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;

public class BookingAnalyticsService : IBookingAnalyticsService
{
    private readonly IRepository<Booking> _bookings;
    private readonly IRepository<Restaurant> _restaurants;

    public BookingAnalyticsService(IRepository<Booking> bookings, IRepository<Restaurant> restaurants)
    {
        _bookings = bookings;
        _restaurants = restaurants;
    }

    public async Task<IReadOnlyList<BookingCountBucket>> GetBookingCountsAsync(int ownerId, AnalyticsPeriod period)
    {
        var restaurants = await _restaurants.GetAllAsync();
        var restaurant = restaurants.FirstOrDefault(r => r.OwnerId == ownerId)
            ?? throw new NotFoundException("You have not set up your restaurant yet.");

        var allBookings = await _bookings.GetAllAsync();

        // Cancelled and declined bookings are not real demand, so they are left out
        var counted = allBookings
            .Where(b => b.RestaurantId == restaurant.Id
                     && b.Status != BookingStatus.Cancelled
                     && b.Status != BookingStatus.Declined)
            .ToList();

        // Every bucket is listed, so quiet days still appear as an empty bar
        var buckets = CreateBuckets(DateOnly.FromDateTime(DateTime.UtcNow), period);

        foreach (var bucket in buckets)
        {
            var inBucket = counted
                .Where(b =>
                {
                    var day = DateOnly.FromDateTime(b.BookingTime);
                    return day >= bucket.Start && day <= bucket.End;
                })
                .ToList();

            bucket.Bookings = inBucket.Count;
            bucket.Guests = inBucket.Sum(b => b.PartySize);
        }

        return buckets;
    }

    // Day view: Monday to Sunday of the current week.
    // Week view: the Monday-start weeks of the current month, with the first and last week cut off at the month's edges
    private static List<BookingCountBucket> CreateBuckets(DateOnly today, AnalyticsPeriod period)
    {
        if (period == AnalyticsPeriod.Day)
        {
            var monday = WeekStart(today);
            return Enumerable.Range(0, 7)
                .Select(offset => new BookingCountBucket { Start = monday.AddDays(offset), End = monday.AddDays(offset) })
                .ToList();
        }

        var firstOfMonth = new DateOnly(today.Year, today.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);
        var buckets = new List<BookingCountBucket>();

        for (var weekStart = WeekStart(firstOfMonth); weekStart <= lastOfMonth; weekStart = weekStart.AddDays(7))
        {
            var weekEnd = weekStart.AddDays(6);
            buckets.Add(new BookingCountBucket
            {
                Start = weekStart < firstOfMonth ? firstOfMonth : weekStart,
                End = weekEnd > lastOfMonth ? lastOfMonth : weekEnd
            });
        }

        return buckets;
    }

    // The Monday on or before a date
    private static DateOnly WeekStart(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));
}
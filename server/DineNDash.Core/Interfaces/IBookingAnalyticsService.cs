using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

public interface IBookingAnalyticsService
{
    Task<IReadOnlyList<BookingCountBucket>> GetBookingCountsAsync(int ownerId, AnalyticsPeriod period);
}
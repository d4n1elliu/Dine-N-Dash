using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Models;
using DineNDash.Core.Services;
using DineNDash.Tests.Fakes;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers how restaurant owners see and respond to bookings in BookingService
/// </summary>
public class BookingServiceTests
{
    private const int OwnerId = 1;
    private const int OtherOwnerId = 2;

    private BookingService _bookingService = null!;
    private InMemoryRepository<Booking> _bookings = null!;
    private InMemoryRepository<Restaurant> _restaurants = null!;
    private InMemoryRepository<DinerAccount> _diners = null!;
    private Restaurant _restaurant = null!;
    private Restaurant _otherRestaurant = null!;
    private DinerAccount _diner = null!;

    [SetUp]
    public async Task SetUp()
    {
        _bookings = new InMemoryRepository<Booking>();
        _restaurants = new InMemoryRepository<Restaurant>();
        _diners = new InMemoryRepository<DinerAccount>();

        var imageService = new ImageService(new InMemoryRepository<Image>(), new InMemoryImageStorage());
        var restaurantService = new RestaurantService(_restaurants, new InMemoryRepository<Review>(), imageService);
        _bookingService = new BookingService(_bookings, _restaurants, _diners, restaurantService);

        _restaurant = await _restaurants.AddAsync(new Restaurant { Name = "Mine", Address = "1 St", Suburb = "Sydney", OwnerId = OwnerId });
        _otherRestaurant = await _restaurants.AddAsync(new Restaurant { Name = "Theirs", Address = "2 St", Suburb = "Sydney", OwnerId = OtherOwnerId });
        _diner = await _diners.AddAsync(new DinerAccount { Email = "dana@example.com", Name = "Dana" });
    }

    private Task<Booking> AddBooking(Restaurant restaurant, BookingStatus status = BookingStatus.Pending, int daysAhead = 1) =>
        _bookings.AddAsync(new Booking
        {
            RestaurantId = restaurant.Id,
            DinerId = _diner.Id,
            BookingTime = DateTime.UtcNow.AddDays(daysAhead),
            PartySize = 2,
            Status = status
        });

    [Test]
    public async Task GetForOwner_ReturnsOnlyTheirBookingsWithDinerNames()
    {
        var later = await AddBooking(_restaurant, daysAhead: 3);
        var sooner = await AddBooking(_restaurant, daysAhead: 1);
        await AddBooking(_otherRestaurant);

        var bookings = (await _bookingService.GetForOwnerAsync(OwnerId)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(bookings.Select(b => b.Id), Is.EqualTo(new[] { sooner.Id, later.Id }));
            Assert.That(bookings.Select(b => b.Diner?.Name), Is.All.EqualTo("Dana"));
        });
    }

    [Test]
    public async Task Respond_Accept_ConfirmsPendingBooking()
    {
        var booking = await AddBooking(_restaurant);

        var updated = await _bookingService.RespondAsync(OwnerId, booking.Id, accept: true);

        Assert.That(updated.Status, Is.EqualTo(BookingStatus.Confirmed));
    }

    [Test]
    public async Task Respond_Decline_DeclinesPendingBooking()
    {
        var booking = await AddBooking(_restaurant);

        var updated = await _bookingService.RespondAsync(OwnerId, booking.Id, accept: false);

        Assert.That(updated.Status, Is.EqualTo(BookingStatus.Declined));
    }

    [Test]
    public async Task Respond_AnotherRestaurantsBooking_ThrowsNotFound()
    {
        var booking = await AddBooking(_otherRestaurant);

        Assert.ThrowsAsync<NotFoundException>(() => _bookingService.RespondAsync(OwnerId, booking.Id, accept: true));
    }

    [TestCase(BookingStatus.Confirmed)]
    [TestCase(BookingStatus.Declined)]
    [TestCase(BookingStatus.Cancelled)]
    public async Task Respond_NotPending_ThrowsBadRequest(BookingStatus status)
    {
        var booking = await AddBooking(_restaurant, status);

        Assert.ThrowsAsync<BadRequestException>(() => _bookingService.RespondAsync(OwnerId, booking.Id, accept: true));
    }

    [Test]
    public async Task Respond_BookingTimePassed_ThrowsBadRequest()
    {
        var booking = await AddBooking(_restaurant);
        booking.BookingTime = DateTime.UtcNow.AddMinutes(-5);

        Assert.ThrowsAsync<BadRequestException>(() => _bookingService.RespondAsync(OwnerId, booking.Id, accept: true));
    }
}

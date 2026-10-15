using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;

/// <summary>
/// Account that books tables and writes reviews
/// </summary>
public class DinerAccount : User
{
    public override UserRole Role => UserRole.Diner;
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
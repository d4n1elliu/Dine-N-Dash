using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;
public class DinerAccount : User
{
    public override UserRole Role => UserRole.Diner;
    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
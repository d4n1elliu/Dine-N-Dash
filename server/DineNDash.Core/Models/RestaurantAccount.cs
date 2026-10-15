using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;

/// <summary>
/// Account that owns and manages one restaurant
/// </summary>
public class RestaurantAccount : User
{
    public override UserRole Role => UserRole.RestaurantOwner;
    public Restaurant? Restaurant { get; set; }
}
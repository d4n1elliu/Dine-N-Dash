using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;
public class RestaurantAccount : User
{
    public override UserRole Role => UserRole.RestaurantOwner;
    public Restaurant? Restaurant { get; set; }
}
namespace DineNDash.Core.Enums;

/// <summary>
/// Serialised by name in the API and the JWT role claim, so renaming a member is a breaking change
/// </summary>
public enum UserRole
{
    Diner,
    RestaurantOwner
}


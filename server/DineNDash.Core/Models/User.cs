using DineNDash.Core.Enums;

namespace DineNDash.Core.Models;

/// <summary>
/// Account fields shared by diners and restaurant owners, stored in one table by EF discriminator
/// </summary>
public abstract class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public abstract UserRole Role { get; }
    public override void Validate()
    {
        Require(!string.IsNullOrWhiteSpace(Email) && Email.Contains('@'), "A valid email is required.");
        Require(!string.IsNullOrWhiteSpace(Name), "Name is required.");
        Require(Name.Length <= 100, "Name must be 100 characters or fewer.");
    }
}
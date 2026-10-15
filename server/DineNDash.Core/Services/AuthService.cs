using DineNDash.Core.Enums;
using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;

/// <summary>
/// Registers accounts and verifies login details using hashed passwords
/// </summary>
public class AuthService(IRepository<User> users, IPasswordHasher passwordHasher) : IAuthService
{
    private const int MinPasswordLength = 8;

    /// <summary>
    /// Rejects short passwords and duplicate emails, then creates the account type for the role with a hashed password
    /// </summary>
    public async Task<User> RegisterAsync(string email, string password, string name, UserRole role)
    {
        email = NormaliseEmail(email);

        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            throw new BadRequestException($"Password must be at least {MinPasswordLength} characters.");

        if (await FindByEmailAsync(email) is not null)
            throw new ConflictException("An account with that email already exists.");

        User user = role switch
        {
            UserRole.Diner => new DinerAccount(),
            UserRole.RestaurantOwner => new RestaurantAccount(),
            _ => throw new BadRequestException("Unknown account type.")
        };

        user.Email = email;
        user.Name = name.Trim();
        user.PasswordHash = passwordHasher.Hash(password);

        return await users.AddAsync(user);
    }

    /// <summary>
    /// Returns the user when the email and password match, otherwise throws Unauthorized without saying which was wrong
    /// </summary>
    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await FindByEmailAsync(NormaliseEmail(email));

        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return user;
    }

    private async Task<User?> FindByEmailAsync(string email)
    {
        var allUsers = await users.GetAllAsync();
        return allUsers.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormaliseEmail(string email) => (email ?? string.Empty).Trim().ToLowerInvariant();
}

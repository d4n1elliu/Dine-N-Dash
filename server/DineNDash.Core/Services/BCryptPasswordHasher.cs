using DineNDash.Core.Interfaces;

namespace DineNDash.Core.Services;

/// <summary>
/// Password hasher backed by BCrypt.
/// </summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Sample data was inserted with plain text passwords, which are never accepted
            return false;
        }
    }
}

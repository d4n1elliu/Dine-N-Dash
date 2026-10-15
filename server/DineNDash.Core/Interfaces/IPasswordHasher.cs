namespace DineNDash.Core.Interfaces;

/// <summary>
/// Produces password hashes for storage and verifies supplied passwords against those hashes.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

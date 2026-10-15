namespace DineNDash.Core.Interfaces;

/// <summary>
/// Produces a password hash for storage and verifies a supplied password against that hash
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

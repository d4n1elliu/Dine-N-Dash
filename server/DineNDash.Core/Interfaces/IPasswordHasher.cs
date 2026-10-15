namespace DineNDash.Core.Interfaces;

/// <summary>
/// Keeps passwords out of the database; only the hash is ever stored or compared
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}

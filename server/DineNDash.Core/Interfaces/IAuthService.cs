using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IAuthService
{
    Task<User> RegisterAsync(string email, string password, string name, UserRole role);
    /// <summary>
    /// Throws UnauthorizedException on a bad email or password without saying which
    /// </summary>
    Task<User> LoginAsync(string email, string password);
}
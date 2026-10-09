using DineNDash.Core.Enums;
using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface IAuthService
{
    Task<User> RegisterAsync(string email, string password, string name, UserRole role);
    Task<User> LoginAsync(string email, string password);
}
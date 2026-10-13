using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;
public interface ITokenService
{
    string CreateToken(User user);
}

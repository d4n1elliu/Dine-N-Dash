using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using DineNDash.Core.Settings;
using Microsoft.IdentityModel.Tokens;

namespace DineNDash.Core.Services;

/// <summary>
/// Token service that signs JWTs with the symmetric key from JwtSettings
/// </summary>
public class JwtTokenService(JwtSettings settings) : ITokenService
{
    /// <summary>
    /// Issues a signed token carrying the user's id, email, name and role that expires after ExpiryMinutes
    /// </summary>
    public string CreateToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

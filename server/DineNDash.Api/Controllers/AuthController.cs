using System.Security.Claims;
using DineNDash.Api.DTOs;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Registers accounts, logs users in and reports who the current token belongs to
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ITokenService tokenService) : ControllerBase
{
    /// <summary>
    /// Creates an account and returns a token for it
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await authService.RegisterAsync(request.Email, request.Password, request.Name, request.Role);
        return StatusCode(StatusCodes.Status201Created, ToAuthResponse(user));
    }

    /// <summary>
    /// Checks credentials and returns a token
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await authService.LoginAsync(request.Email, request.Password);
        return Ok(ToAuthResponse(user));
    }

    /// <summary>
    /// Returns the user behind the current token
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new UserResponse
        {
            Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
            Name = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            Email = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            Role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty
        });
    }

    private AuthResponse ToAuthResponse(User user) => new()
    {
        Token = tokenService.CreateToken(user),
        User = new UserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        }
    };
}

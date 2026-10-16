using System.Security.Claims;

namespace DineNDash.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    // Reads the logged-in user's ID from the token
    public static int GetUserId(this ClaimsPrincipal user) =>
        int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
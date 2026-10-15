using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Lets the frontend and deployment checks confirm the API is up
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Returns a fixed healthy status
    /// </summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "Healthy" });
}
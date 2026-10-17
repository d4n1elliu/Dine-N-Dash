using DineNDash.Core.Interfaces;
using DineNDash.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DineNDash.Api.Controllers;

/// <summary>
/// Serves uploaded images; uploads happen through the restaurant and menu endpoints that own them
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    /// <summary>
    /// Largest request body an upload endpoint accepts: the image limit plus room for the form around it
    /// </summary>
    public const long MaxUploadRequestBytes = ImageService.MaxSizeBytes + 64 * 1024;

    private IImageService _imageService;

    public ImageController(IImageService imageService)
    {
        _imageService = imageService;
    }

    /// <summary>
    /// Builds the link the client uses to show an image, or null when there is none
    /// </summary>
    public static string? UrlFor(int? imageId) => imageId == null ? null : $"/api/image/{imageId}";

    /// <summary>
    /// Returns the image file; a replaced image gets a new id, so it can be cached for good
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var (image, content) = await _imageService.OpenAsync(id);
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(content, image.ContentType);
    }
}

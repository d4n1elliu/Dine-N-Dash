using DineNDash.Core.Models;

namespace DineNDash.Core.Interfaces;

/// <summary>
/// Stores uploaded images and keeps the Image rows and files in step
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Throws BadRequestException when the upload is empty, too large or not a JPEG, PNG or WebP image
    /// </summary>
    Task<Image> UploadAsync(Stream content, string originalFileName);
    /// <summary>
    /// Throws NotFoundException when the image does not exist
    /// </summary>
    Task<(Image Image, Stream Content)> OpenAsync(int id);
    /// <summary>
    /// Removes the row and the file; does nothing when the image is already gone
    /// </summary>
    Task DeleteAsync(int id);
}

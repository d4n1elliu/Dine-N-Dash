namespace DineNDash.Core.Interfaces;

/// <summary>
/// Saves image files outside the database; backed by a local folder in the API and an in-memory store in tests
/// </summary>
public interface IImageStorage
{
    Task SaveAsync(string fileName, Stream content);
    /// <summary>
    /// Throws NotFoundException when the file is missing
    /// </summary>
    Task<Stream> OpenReadAsync(string fileName);
    /// <summary>
    /// Does nothing when the file is already gone
    /// </summary>
    Task DeleteAsync(string fileName);
}

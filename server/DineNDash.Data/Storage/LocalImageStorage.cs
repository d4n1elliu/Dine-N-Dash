using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;

namespace DineNDash.Data.Storage;

/// <summary>
/// Keeps image files in a folder on the machine the API runs on
/// </summary>
public class LocalImageStorage : IImageStorage
{
    private readonly string _rootPath;

    public LocalImageStorage(string rootPath)
    {
        _rootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(_rootPath);
    }

    public async Task SaveAsync(string fileName, Stream content)
    {
        await using var file = new FileStream(ResolvePath(fileName), FileMode.CreateNew, FileAccess.Write);
        await content.CopyToAsync(file);
    }

    public Task<Stream> OpenReadAsync(string fileName)
    {
        var path = ResolvePath(fileName);
        if (!File.Exists(path))
        {
            throw new NotFoundException("Unable to find image file.");
        }

        return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteAsync(string fileName)
    {
        File.Delete(ResolvePath(fileName));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Only accepts a bare file name so a stored name can never point outside the storage folder
    /// </summary>
    private string ResolvePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || Path.GetFileName(fileName) != fileName)
        {
            throw new BadRequestException("Invalid image file name.");
        }

        return Path.Combine(_rootPath, fileName);
    }
}

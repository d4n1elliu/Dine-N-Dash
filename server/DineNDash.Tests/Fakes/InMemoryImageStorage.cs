using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;

namespace DineNDash.Tests.Fakes;

/// <summary>
/// In-memory IImageStorage used by tests instead of the uploads folder
/// </summary>
public class InMemoryImageStorage : IImageStorage
{
    public Dictionary<string, byte[]> Files { get; } = [];

    public async Task SaveAsync(string fileName, Stream content)
    {
        using var copy = new MemoryStream();
        await content.CopyToAsync(copy);
        Files.Add(fileName, copy.ToArray());
    }

    public Task<Stream> OpenReadAsync(string fileName) =>
        Files.TryGetValue(fileName, out var bytes)
            ? Task.FromResult<Stream>(new MemoryStream(bytes))
            : throw new NotFoundException("Unable to find image file.");

    public Task DeleteAsync(string fileName)
    {
        Files.Remove(fileName);
        return Task.CompletedTask;
    }
}

using DineNDash.Core.Exceptions;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Core.Services;
/// <summary>
/// Image rules: size limit, allowed formats checked from the file's contents, and unique stored file names
/// </summary>
public class ImageService : IImageService
{
    public const long MaxSizeBytes = 5 * 1024 * 1024;

    private IRepository<Image> _images;
    private IImageStorage _storage;

    public ImageService(IRepository<Image> images, IImageStorage storage)
    {
        _images = images;
        _storage = storage;
    }

    /// <summary>
    /// Checks the size and format, saves the file under a new name, then records it; the file is removed again if saving the row fails
    /// </summary>
    public async Task<Image> UploadAsync(Stream content, string originalFileName)
    {
        using var buffer = new MemoryStream();
        await CopyWithLimitAsync(content, buffer);

        if (buffer.Length == 0)
        {
            throw new BadRequestException("The image is empty.");
        }

        var (contentType, extension) = DetectFormat(buffer.GetBuffer().AsSpan(0, (int)buffer.Length))
            ?? throw new BadRequestException("Only JPEG, PNG and WebP images are allowed.");

        var image = new Image
        {
            FileName = $"{Guid.NewGuid():N}{extension}",
            OriginalFileName = Path.GetFileName(originalFileName ?? string.Empty),
            ContentType = contentType,
            SizeBytes = buffer.Length
        };
        image.Validate();

        buffer.Position = 0;
        await _storage.SaveAsync(image.FileName, buffer);

        try
        {
            return await _images.AddAsync(image);
        }
        catch
        {
            await _storage.DeleteAsync(image.FileName);
            throw;
        }
    }

    /// <summary>
    /// Returns the image details with its file contents
    /// </summary>
    public async Task<(Image Image, Stream Content)> OpenAsync(int id)
    {
        var image = await _images.GetByIdAsync(id) ?? throw new NotFoundException("Unable to find image.");
        return (image, await _storage.OpenReadAsync(image.FileName));
    }

    /// <summary>
    /// Deletes the row first so nothing points at a missing file, then the file
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var image = await _images.GetByIdAsync(id);
        if (image == null)
        {
            return;
        }

        await _images.DeleteAsync(id);
        await _storage.DeleteAsync(image.FileName);
    }

    private static async Task CopyWithLimitAsync(Stream source, MemoryStream destination)
    {
        var chunk = new byte[81920];
        int read;
        while ((read = await source.ReadAsync(chunk)) > 0)
        {
            if (destination.Length + read > MaxSizeBytes)
            {
                throw new BadRequestException($"Images must be {MaxSizeBytes / (1024 * 1024)} MB or smaller.");
            }
            destination.Write(chunk, 0, read);
        }
    }

    /// <summary>
    /// Reads the file signature so a renamed or mislabelled file cannot pass as an image
    /// </summary>
    private static (string ContentType, string Extension)? DetectFormat(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith((byte[])[0xFF, 0xD8, 0xFF]))
        {
            return ("image/jpeg", ".jpg");
        }

        if (bytes.StartsWith((byte[])[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]))
        {
            return ("image/png", ".png");
        }

        if (bytes.Length >= 12 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
        {
            return ("image/webp", ".webp");
        }

        return null;
    }
}

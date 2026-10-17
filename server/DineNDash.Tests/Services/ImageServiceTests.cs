using DineNDash.Core.Exceptions;
using DineNDash.Core.Models;
using DineNDash.Core.Services;
using DineNDash.Tests.Fakes;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers upload checks, stored file names and deletion in ImageService
/// </summary>
public class ImageServiceTests
{
    private ImageService _imageService = null!;
    private InMemoryRepository<Image> _images = null!;
    private InMemoryImageStorage _storage = null!;

    [SetUp]
    public void SetUp()
    {
        _images = new InMemoryRepository<Image>();
        _storage = new InMemoryImageStorage();
        _imageService = new ImageService(_images, _storage);
    }

    [Test]
    public async Task Upload_Png_SavesFileAndRecordsImage()
    {
        var image = await _imageService.UploadAsync(TestImages.Png(), "photo.png");

        Assert.Multiple(() =>
        {
            Assert.That(image.Id, Is.GreaterThan(0));
            Assert.That(image.ContentType, Is.EqualTo("image/png"));
            Assert.That(image.FileName, Does.EndWith(".png"));
            Assert.That(image.OriginalFileName, Is.EqualTo("photo.png"));
            Assert.That(_storage.Files.ContainsKey(image.FileName), Is.True);
        });
    }

    [Test]
    public async Task Upload_DetectsFormatFromContentsNotFileName()
    {
        var jpeg = await _imageService.UploadAsync(TestImages.Jpeg(), "actually-a-jpeg.png");
        var webp = await _imageService.UploadAsync(TestImages.WebP(), "picture");

        Assert.Multiple(() =>
        {
            Assert.That(jpeg.ContentType, Is.EqualTo("image/jpeg"));
            Assert.That(jpeg.FileName, Does.EndWith(".jpg"));
            Assert.That(webp.ContentType, Is.EqualTo("image/webp"));
        });
    }

    [Test]
    public async Task Upload_StoresUnderGeneratedNameNotClientPath()
    {
        var image = await _imageService.UploadAsync(TestImages.Png(), "../../etc/passwd.png");

        Assert.Multiple(() =>
        {
            Assert.That(image.FileName, Does.Not.Contain("passwd"));
            Assert.That(image.FileName, Does.Not.Contain("/"));
            Assert.That(image.OriginalFileName, Is.EqualTo("passwd.png"));
        });
    }

    [Test]
    public void Upload_NotAnImage_ThrowsBadRequestAndSavesNothing()
    {
        Assert.ThrowsAsync<BadRequestException>(() =>
            _imageService.UploadAsync(new MemoryStream("<svg></svg>"u8.ToArray()), "drawing.png"));

        Assert.That(_storage.Files, Is.Empty);
    }

    [Test]
    public void Upload_Empty_ThrowsBadRequest()
    {
        Assert.ThrowsAsync<BadRequestException>(() => _imageService.UploadAsync(new MemoryStream(), "empty.png"));
    }

    [Test]
    public void Upload_TooLarge_ThrowsBadRequest()
    {
        var tooLarge = new byte[ImageService.MaxSizeBytes + 1];
        TestImages.Png().ToArray().CopyTo(tooLarge, 0);

        Assert.ThrowsAsync<BadRequestException>(() => _imageService.UploadAsync(new MemoryStream(tooLarge), "huge.png"));
        Assert.That(_storage.Files, Is.Empty);
    }

    [Test]
    public async Task Delete_RemovesRowAndFile()
    {
        var image = await _imageService.UploadAsync(TestImages.Png(), "photo.png");

        await _imageService.DeleteAsync(image.Id);
        var stored = await _images.GetByIdAsync(image.Id);

        Assert.Multiple(() =>
        {
            Assert.That(stored, Is.Null);
            Assert.That(_storage.Files, Is.Empty);
        });
    }

    [Test]
    public void Open_MissingImage_ThrowsNotFound()
    {
        Assert.ThrowsAsync<NotFoundException>(() => _imageService.OpenAsync(42));
    }
}

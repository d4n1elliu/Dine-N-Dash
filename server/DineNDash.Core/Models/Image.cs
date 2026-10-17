namespace DineNDash.Core.Models;

/// <summary>
/// An uploaded image file saved in the backend's storage folder; the row tracks the file so it can be served and deleted
/// </summary>
public class Image : BaseEntity
{
    /// <summary>
    /// Name of the file in storage, generated on upload so it is unique and safe to use as a path
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    /// <summary>
    /// Worked out from the file's contents on upload, not from what the client claims
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public override void Validate()
    {
        Require(!string.IsNullOrWhiteSpace(FileName), "Image file name is required.");
        Require(!string.IsNullOrWhiteSpace(ContentType), "Image content type is required.");
        Require(SizeBytes > 0, "Image cannot be empty.");
    }
}

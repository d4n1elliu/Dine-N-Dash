using DineNDash.Core.Exceptions;

namespace DineNDash.Core.Models;

/// <summary>
/// Shared id and timestamps for every row, plus the validation hook repositories call before saving
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    /// <summary>
    /// Throws BadRequestException when the entity breaks its own rules; repositories call this before any insert or update
    /// </summary>
    public abstract void Validate();
    protected static void Require(bool condition, string message)
    {
        if (!condition)
            throw new BadRequestException(message);
    }
}
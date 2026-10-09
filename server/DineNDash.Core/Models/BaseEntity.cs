using DineNDash.Core.Exceptions;

namespace DineNDash.Core.Models;
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public abstract void Validate();
    protected static void Require(bool condition, string message)
    {
        if (!condition)
            throw new BadRequestException(message);
    }
}
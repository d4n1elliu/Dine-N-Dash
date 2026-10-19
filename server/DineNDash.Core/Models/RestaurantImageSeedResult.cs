namespace DineNDash.Core.Models;

/// <summary>
/// What one seed run did, so the caller can log it
/// </summary>
public class RestaurantImageSeedResult
{
    public int Seeded { get; set; }
    public int Skipped { get; set; }
    public List<string> Unmatched { get; } = [];
    public List<string> Ambiguous { get; } = [];
}

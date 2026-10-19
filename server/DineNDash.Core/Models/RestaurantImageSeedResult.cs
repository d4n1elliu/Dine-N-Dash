namespace DineNDash.Core.Models;

/// <summary>
/// What one seed run did, so the caller can log it
/// </summary>
public class RestaurantImageSeedResult
{
    public int Seeded { get; set; }
    /// <summary>
    /// Restaurants that already had a profile image and were left alone
    /// </summary>
    public int Skipped { get; set; }
    /// <summary>
    /// Restaurant names from the seed list that do not exist in the database
    /// </summary>
    public List<string> Unmatched { get; } = [];
}

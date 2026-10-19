using DineNDash.Api.Seed;
using DineNDash.Core.Interfaces;
using DineNDash.Core.Models;

namespace DineNDash.Api.Extensions;

/// <summary>
/// Seeds the starting photos for the sample restaurants when the API starts
/// </summary>
public static class SeedExtensions
{
    /// <summary>
    /// Uploads each photo in Seed/Images to its restaurant; a missing file or restaurant is logged and never stops startup
    /// </summary>
    public static async Task SeedRestaurantImagesAsync(this WebApplication app)
    {
        var folder = Path.Combine(app.Environment.ContentRootPath, "Seed", "Images");
        if (!Directory.Exists(folder))
        {
            app.Logger.LogWarning("Seed images folder {Folder} not found, skipping restaurant images", folder);
            return;
        }

        var seeds = new List<RestaurantSeedImage>();
        foreach (var (fileName, restaurantName) in RestaurantImageSeeds.FileToRestaurantName)
        {
            var path = Path.Combine(folder, fileName);
            if (!File.Exists(path))
            {
                app.Logger.LogWarning("Seed image {File} for {Restaurant} is missing", fileName, restaurantName);
                continue;
            }

            seeds.Add(new RestaurantSeedImage(restaurantName, fileName, () => File.OpenRead(path)));
        }

        try
        {
            using var scope = app.Services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IRestaurantImageSeeder>();
            var result = await seeder.SeedAsync(seeds);

            app.Logger.LogInformation("Restaurant images: {Seeded} seeded, {Skipped} already had one", result.Seeded, result.Skipped);
            foreach (var name in result.Unmatched)
            {
                app.Logger.LogWarning("No restaurant named {Restaurant} to seed an image for", name);
            }
        }
        catch (Exception exception)
        {
            app.Logger.LogError(exception, "Seeding restaurant images failed");
        }
    }
}

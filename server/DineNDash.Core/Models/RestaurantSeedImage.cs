namespace DineNDash.Core.Models;

/// <summary>
/// One starting photo: the restaurant it belongs to, the file name to record and a way to open its contents when needed
/// </summary>
public record RestaurantSeedImage(string RestaurantName, string FileName, Func<Stream> Open);

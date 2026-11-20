namespace DineNDash.Core.Services;

/// <summary>
/// Generic k-nearest neighbours: ranks items by their distance to a target
/// </summary>
public static class Knn
{
    /// <summary>
    /// Returns the k candidates closest to the target, nearest first.
    /// Ties go to the tie-breaker, then to the original order. Excluding the target itself is the caller's job
    /// </summary>
    public static List<T> FindNearest<T>(
        T target,
        IEnumerable<T> candidates,
        int k,
        Func<T, T, double> distance,
        IComparer<T>? tieBreaker = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(k, 1);

        var ranked = candidates.OrderBy(candidate => distance(target, candidate));

        if (tieBreaker != null)
        {
            ranked = ranked.ThenBy(candidate => candidate, tieBreaker);
        }

        return ranked.Take(k).ToList();
    }
}

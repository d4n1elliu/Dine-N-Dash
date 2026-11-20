using DineNDash.Core.Services;

namespace DineNDash.Tests.Services;

/// <summary>
/// Covers the generic KNN ranking using plain numbers
/// </summary>
public class KnnTests
{
    private static double Gap(int a, int b) => Math.Abs(a - b);

    [Test]
    public void FindNearest_ReturnsClosestFirst()
    {
        var result = Knn.FindNearest(10, [1, 9, 12, 30], 3, Gap);

        Assert.That(result, Is.EqualTo(new[] { 9, 12, 1 }));
    }

    [Test]
    public void FindNearest_FewerCandidatesThanK_ReturnsAll()
    {
        var result = Knn.FindNearest(10, [8, 11], 5, Gap);

        Assert.That(result, Is.EqualTo(new[] { 11, 8 }));
    }

    [Test]
    public void FindNearest_NoCandidates_ReturnsEmpty()
    {
        var result = Knn.FindNearest(10, Array.Empty<int>(), 5, Gap);

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FindNearest_Tie_UsesTieBreaker()
    {
        var largerFirst = Comparer<int>.Create((a, b) => b.CompareTo(a));

        var result = Knn.FindNearest(10, [8, 12], 2, Gap, largerFirst);

        Assert.That(result, Is.EqualTo(new[] { 12, 8 }));
    }

    [Test]
    public void FindNearest_TieWithoutTieBreaker_KeepsOriginalOrder()
    {
        var result = Knn.FindNearest(10, [12, 8], 2, Gap);

        Assert.That(result, Is.EqualTo(new[] { 12, 8 }));
    }

    [Test]
    public void FindNearest_DistanceBeatsTieBreaker()
    {
        var largerFirst = Comparer<int>.Create((a, b) => b.CompareTo(a));

        var result = Knn.FindNearest(10, [20, 9], 1, Gap, largerFirst);

        Assert.That(result, Is.EqualTo(new[] { 9 }));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void FindNearest_KBelowOne_Throws(int k)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Knn.FindNearest(10, [1, 2], k, Gap));
    }
}

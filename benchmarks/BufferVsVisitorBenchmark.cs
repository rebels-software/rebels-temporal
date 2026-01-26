using BenchmarkDotNet.Attributes;
using Rebels.Temporal;

namespace Rebels.Temporal.Benchmarks;

/// <summary>
/// Benchmark for visitor-based matching at various scales.
/// </summary>
/// <remarks>
/// Historical comparison between buffer-based and visitor-based APIs
/// showed 1-4% performance difference (within measurement noise).
/// See ADR-10 for detailed benchmark results.
/// </remarks>
[MemoryDiagnoser]
public class VisitorScaleBenchmark
{
    [Params(100, 1_000, 10_000)]
    public int Count { get; set; }

    private TestPoint[] _anchors = null!;
    private TestPoint[] _candidates = null!;
    private MatchPair<TestPoint, TestPoint>[] _buffer = null!;
    private MatchPolicy _sortedPolicy = null!;
    private MatchPolicy _unsortedPolicy = null!;

    [GlobalSetup]
    public void Setup()
    {
        _anchors = Enumerable.Range(0, Count)
            .Select(i => new TestPoint(DateTimeOffset.UnixEpoch.AddSeconds(i)))
            .ToArray();

        _candidates = Enumerable.Range(0, Count)
            .Select(i => new TestPoint(DateTimeOffset.UnixEpoch.AddSeconds(i)))
            .ToArray();

        _buffer = new MatchPair<TestPoint, TestPoint>[Count];

        _sortedPolicy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.None,
            InputOrdering = InputOrdering.Both
        };

        _unsortedPolicy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.None,
            InputOrdering = InputOrdering.None
        };
    }

    // ===========================================
    // Sorted Input - O(n+m) dual-pointer scan
    // ===========================================

    [Benchmark]
    [BenchmarkCategory("Sorted")]
    public int Sorted()
    {
        var visitor = new BufferVisitor<TestPoint, TestPoint>(_buffer);
        MatchTemporal.Points.With.Points<TestPoint, TestPoint, BufferVisitor<TestPoint, TestPoint>>(
            _anchors, _candidates, _sortedPolicy, ref visitor);
        return visitor.MatchCount;
    }

    // ===========================================
    // Unsorted Input - O(n*m) nested loops
    // ===========================================

    [Benchmark]
    [BenchmarkCategory("Unsorted")]
    public int Unsorted()
    {
        var visitor = new BufferVisitor<TestPoint, TestPoint>(_buffer);
        MatchTemporal.Points.With.Points<TestPoint, TestPoint, BufferVisitor<TestPoint, TestPoint>>(
            _anchors, _candidates, _unsortedPolicy, ref visitor);
        return visitor.MatchCount;
    }
}

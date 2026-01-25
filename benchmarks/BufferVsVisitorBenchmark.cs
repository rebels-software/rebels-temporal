using BenchmarkDotNet.Attributes;
using Rebels.Temporal;

namespace Rebels.Temporal.Benchmarks;

/// <summary>
/// Benchmark comparing buffer-based API vs visitor-based API performance.
/// </summary>
[MemoryDiagnoser]
public class BufferVsVisitorBenchmark
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

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("Sorted")]
    public int Buffer_Sorted()
    {
        var buffer = new MatchBuffer<TestPoint, TestPoint> { Pairs = _buffer };
        return MatchTemporal.Points.With.Points(_anchors, _candidates, _sortedPolicy, ref buffer);
    }

    [Benchmark]
    [BenchmarkCategory("Sorted")]
    public int Visitor_Sorted()
    {
        var visitor = new BufferVisitor<TestPoint, TestPoint>(_buffer);
        return MatchTemporal.Points.With.Points<TestPoint, TestPoint, BufferVisitor<TestPoint, TestPoint>>(
            _anchors, _candidates, _sortedPolicy, ref visitor);
    }

    // ===========================================
    // Unsorted Input - O(n*m) nested loops
    // ===========================================

    [Benchmark]
    [BenchmarkCategory("Unsorted")]
    public int Buffer_Unsorted()
    {
        var buffer = new MatchBuffer<TestPoint, TestPoint> { Pairs = _buffer };
        return MatchTemporal.Points.With.Points(_anchors, _candidates, _unsortedPolicy, ref buffer);
    }

    [Benchmark]
    [BenchmarkCategory("Unsorted")]
    public int Visitor_Unsorted()
    {
        var visitor = new BufferVisitor<TestPoint, TestPoint>(_buffer);
        return MatchTemporal.Points.With.Points<TestPoint, TestPoint, BufferVisitor<TestPoint, TestPoint>>(
            _anchors, _candidates, _unsortedPolicy, ref visitor);
    }
}

using BenchmarkDotNet.Attributes;
using Rebels.Temporal;

namespace Rebels.Temporal.Benchmarks;

[MemoryDiagnoser]
public class PointMatchingUnsortedBenchmark
{
    private const int Count = 2_000;

    private TestPoint[] _anchors = null!;
    private TestPoint[] _candidates = null!;
    private MatchPair<TestPoint, TestPoint>[] _buffer = null!;
    private MatchPolicy _policy = null!;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random(42);
        var timestamps = Enumerable.Range(0, Count)
            .Select(i => DateTimeOffset.UnixEpoch.AddSeconds(i))
            .OrderBy(_ => random.Next())
            .ToArray();

        _anchors = timestamps
            .Select(t => new TestPoint(t))
            .ToArray();

        _candidates = timestamps
            .Select(t => new TestPoint(t))
            .OrderBy(_ => random.Next())
            .ToArray();

        _buffer = new MatchPair<TestPoint, TestPoint>[Count];

        _policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.None,
            InputOrdering = InputOrdering.None
        };
    }

    [Benchmark]
    public int MatchUnsorted()
    {
        var buffer = new MatchBuffer<TestPoint, TestPoint> { Pairs = _buffer };
        return MatchTemporal.Points.With.Points(_anchors, _candidates, _policy, ref buffer);
    }
}

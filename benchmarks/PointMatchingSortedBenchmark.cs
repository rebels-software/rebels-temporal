using BenchmarkDotNet.Attributes;
using Rebels.Temporal;

namespace Rebels.Temporal.Benchmarks;

[MemoryDiagnoser]
public class PointMatchingSortedBenchmark
{
    private const int Count = 2_000;

    private TestPoint[] _anchors = null!;
    private TestPoint[] _candidates = null!;
    private MatchPair<TestPoint, TestPoint>[] _buffer = null!;
    private MatchPolicy _policy = null!;

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

        _policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.None,
            InputOrdering = InputOrdering.Both
        };
    }

    [Benchmark]
    public int MatchSorted()
    {
        var visitor = new BufferVisitor<TestPoint, TestPoint>(_buffer);
        MatchTemporal.Points.With.Points<TestPoint, TestPoint, BufferVisitor<TestPoint, TestPoint>>(
            _anchors, _candidates, _policy, ref visitor);
        return visitor.MatchCount;
    }
}

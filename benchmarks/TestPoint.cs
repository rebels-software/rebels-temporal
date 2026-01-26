using Rebels.Temporal;

namespace Rebels.Temporal.Benchmarks;

public readonly struct TestPoint : ITemporalPoint
{
    public DateTimeOffset At { get; }

    public TestPoint(int seconds) => At = DateTimeOffset.UnixEpoch.AddSeconds(seconds);

    public TestPoint(DateTimeOffset at) => At = at;
}

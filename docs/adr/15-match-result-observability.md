# ADR-15 — Match Result Observability

## Status
Accepted (implemented via Visitor Pattern)

## Context
In production IoT and telemetry systems, operators need visibility into matching operations:
- How many anchors found matches?
- How many anchors found NO matches (unmatched)?
- What is the match distribution?

A buffer-based API returning only match count provides no insight into:
- Which anchors didn't find any candidates
- Whether the match count is "good" relative to input sizes

### The Core Problem
Calculating "unmatched anchors" post-hoc is non-trivial:
- One anchor may match 0, 1, or many candidates
- `matchCount` is total pairs, not unique anchors
- Post-processing requires allocation:
```csharp
var matchedAnchors = new HashSet<TAnchor>();
for (int i = 0; i < matchCount; i++)
    matchedAnchors.Add(buffer[i].Anchor);
int unmatchedCount = anchors.Length - matchedAnchors.Count;
```
This allocates and has O(n) overhead — against the zero-allocation principle (INV-3).

## Options Considered

### Option 1: Return `MatchResult` struct instead of `int`

```csharp
public readonly struct MatchResult
{
    public int MatchCount { get; }
    public int MatchedAnchorCount { get; }
    public int UnmatchedAnchorCount { get; }
}
```

| Aspect | Assessment |
|--------|------------|
| Performance | Zero allocation (struct) |
| API change | Breaking (return type changes) |
| Flexibility | Low — fixed set of metrics |

### Option 2: `out MatchMetrics` parameter

```csharp
int count = MatchTemporal.Points.With.Points(
    anchors, candidates, policy, ref buffer, out MatchMetrics metrics);
```

| Aspect | Assessment |
|--------|------------|
| Performance | Zero allocation (struct) |
| API change | Non-breaking (overload) |
| Flexibility | Low — fixed set of metrics |

### Option 3: Visitor Pattern with `OnUnmatchedAnchor`

```csharp
public interface IMatchVisitor<TAnchor, TCandidate>
{
    void OnMatch(in TAnchor anchor, in TCandidate candidate, ...);
    void OnUnmatchedAnchor(in TAnchor anchor, int anchorIndex);
}
```

| Aspect | Assessment |
|--------|------------|
| Performance | Zero allocation (struct visitor) |
| API change | New API alongside existing |
| Flexibility | High — user controls what to track |

### Option 4: Callback parameter

```csharp
int count = MatchTemporal.Points.With.Points(
    anchors, candidates, policy, ref buffer,
    onUnmatchedAnchor: (anchor, index) => { ... });
```

| Aspect | Assessment |
|--------|------------|
| Performance | Delegate overhead; potential closure allocations |
| API change | Non-breaking (overload) |
| Flexibility | Medium |

## Decision
Adopt **Option 3: Visitor Pattern** as the primary API.

The visitor interface provides both match notifications and unmatched anchor tracking:

```csharp
public interface IMatchVisitor<TAnchor, TCandidate>
{
    void OnMatch(
        in TAnchor anchor,
        in TCandidate candidate,
        int anchorIndex,
        int candidateIndex,
        MatchType type,
        TemporalRelation? relation);

    void OnUnmatchedAnchor(in TAnchor anchor, int anchorIndex);
}
```

### Usage Example

```csharp
public struct MetricsVisitor<TA, TC> : IMatchVisitor<TA, TC>
{
    public int MatchCount;
    public int UnmatchedCount;

    public void OnMatch(...) => MatchCount++;
    public void OnUnmatchedAnchor(...) => UnmatchedCount++;
}

var visitor = new MetricsVisitor<Event, Event>();
MatchTemporal.Points.With.Points(anchors, candidates, policy, ref visitor);

Console.WriteLine($"Matches: {visitor.MatchCount}");
Console.WriteLine($"Unmatched anchors: {visitor.UnmatchedCount}");
Console.WriteLine($"Match rate: {100.0 * visitor.MatchCount / anchors.Length:F1}%");
```

## Benchmark Results

Visitor pattern with observability vs buffer-based API without observability:

### Environment
- .NET 9.0.11, Windows 11
- Intel Core i7-1185G7 3.00GHz
- BenchmarkDotNet v0.13.12

### Results (both writing to same backing array)

| Count | Buffer (no metrics) | Visitor (with metrics) | Overhead |
|-------|---------------------|------------------------|----------|
| 100   | 2.93 μs | 3.04 μs | +4% |
| 1,000 | 27.6 μs | 28.3 μs | +3% |
| 10,000 | 282 μs | 285 μs | +1% |

### Analysis

1. **Overhead: 1-4%** — within measurement noise, statistically insignificant
2. **Zero additional allocations** — struct visitor is fully inlined
3. **Full observability included** — `OnUnmatchedAnchor` called for every anchor without match
4. **No opt-in complexity** — observability is always available, user decides what to track

## Consequences

- Unmatched anchor tracking is built into the primary API
- No performance penalty for observability (benchmarked)
- Users implement custom visitors for specific metrics needs
- Reference implementations provided (`BufferVisitor` with `UnmatchedCount`)
- No separate "diagnostic mode" or configuration flags needed
- Zero-allocation guarantee maintained (INV-3)

## Related
- [ADR-10 — Matching Output Strategy](10-user-provided-buffer-strategy.md) — visitor pattern decision
- [ADR-14 — Confidence Scoring (Deferred)](14-confidence-scoring-deferred.md)
- [INV-3 — No Allocations in Hot Path](../invariants/3-no-allocations-in-hot-path.md)

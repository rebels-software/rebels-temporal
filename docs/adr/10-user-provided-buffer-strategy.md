# ADR-10 — Matching Output Strategy

## Status
Accepted

## Context
IoT systems may process thousands of temporal events per second. Returning results as `IEnumerable<T>` introduces allocations (enumerators, lists, closures) and generates GC pressure. High-throughput consumers require deterministic memory usage, especially when match operations are invoked frequently.

Additionally, production systems need observability into matching operations:
- How many anchors found matches?
- How many anchors found NO matches (unmatched)?
- What is the match distribution?

These requirements drove evaluation of multiple API approaches.

## Options Considered

### Option 1: Return `List<T>` or `IEnumerable<T>`

```csharp
IEnumerable<MatchPair<TAnchor, TCandidate>> Match(anchors, candidates, policy);
```

| Aspect | Assessment |
|--------|------------|
| Performance | Poor — allocates on every call |
| Observability | None — no unmatched anchor tracking |
| API simplicity | High |

**Rejected** — violates INV-3 (no allocations in hot path).

### Option 2: User-Provided Buffer (`ref struct`)

```csharp
int count = MatchTemporal.Points.With.Points(
    anchors, candidates, policy, ref buffer);
```

| Aspect | Assessment |
|--------|------------|
| Performance | Excellent — zero allocations |
| Observability | None — only returns match count |
| API simplicity | Medium — user manages buffer sizing |

**Considered** — meets performance requirements but lacks observability.

### Option 3: Visitor Pattern with Struct Constraint

```csharp
int count = MatchTemporal.Points.With.Points<TAnchor, TCandidate, TVisitor>(
    anchors, candidates, policy, ref visitor);
```

| Aspect | Assessment |
|--------|------------|
| Performance | Excellent — JIT devirtualizes struct visitors |
| Observability | Full — `OnMatch` + `OnUnmatchedAnchor` callbacks |
| API simplicity | Medium — user implements visitor |

**Selected** — combines zero-allocation performance with full observability.

## Decision
Adopt the **Visitor Pattern** as the primary matching API:

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

### Method Signature

```csharp
public int Points<TAnchor, TCandidate, TVisitor>(
    ReadOnlySpan<TAnchor> anchors,
    ReadOnlySpan<TCandidate> candidates,
    MatchPolicy policy,
    ref TVisitor visitor)
    where TAnchor : ITemporalPoint
    where TCandidate : ITemporalPoint
    where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
```

The `allows ref struct` constraint (C# 13 / .NET 9) enables:
- Regular structs with heap-allocated backing arrays
- Ref structs with stack-allocated `Span<T>` backing
- JIT devirtualization and inlining for maximum performance

### Reference Implementation

```csharp
public ref struct BufferVisitor<TAnchor, TCandidate> : IMatchVisitor<TAnchor, TCandidate>
{
    private readonly Span<MatchPair<TAnchor, TCandidate>> _pairs;
    public int MatchCount;
    public int UnmatchedCount;

    public BufferVisitor(Span<MatchPair<TAnchor, TCandidate>> pairs) { ... }

    public void OnMatch(...) => _pairs[MatchCount++] = new MatchPair(...);
    public void OnUnmatchedAnchor(...) => UnmatchedCount++;
}
```

## Benchmark Results

Comparison of buffer-based API vs visitor-based API with identical workloads:

### Environment
- .NET 9.0.11, Windows 11
- Intel Core i7-1185G7 3.00GHz
- BenchmarkDotNet v0.13.12

### Sorted Input (O(n+m) dual-pointer algorithm)

| Count | Buffer | Visitor | Ratio | Allocated |
|-------|--------|---------|-------|-----------|
| 100   | 2.93 μs | 3.04 μs | 1.04 | 0 B |
| 1,000 | 27.6 μs | 28.3 μs | 1.03 | 0 B |
| 10,000 | 282 μs | 285 μs | 1.01 | 0 B |

### Unsorted Input (O(n×m) nested loops)

| Count | Buffer | Visitor | Ratio | Allocated |
|-------|--------|---------|-------|-----------|
| 100   | 17.6 μs | 17.1 μs | 0.97 | 0 B |
| 1,000 | 1,175 μs | 1,189 μs | 1.01 | ~1 B |
| 10,000 | 137.4 ms | 138.5 ms | 1.01 | ~100 B |

### Analysis

1. **Performance difference: 1-4%** — within measurement noise, statistically insignificant
2. **Zero allocations maintained** — both approaches show negligible allocations (measurement noise)
3. **JIT optimization effective** — `allows ref struct` + generic constraint enables full devirtualization
4. **Visitor provides observability at no cost** — `OnUnmatchedAnchor` tracking included

## Consequences

- Visitor pattern is the primary public API
- Zero-allocation guarantee maintained (INV-3)
- Users gain built-in observability (unmatched anchor tracking)
- Library requires .NET 9+ / C# 13 for `allows ref struct` feature
- Callers implement `IMatchVisitor<TAnchor, TCandidate>` for custom result handling
- Reference implementations provided for common use cases (`BufferVisitor`)

## Related
- [ADR-15 — Match Result Observability](15-match-result-observability.md)
- [INV-3 — No Allocations in Hot Path](../invariants/3-no-allocations-in-hot-path.md)

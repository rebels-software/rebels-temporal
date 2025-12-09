# ADR-11 — Result Pooling Strategy

## Status
Accepted

## Context
IoT systems may process thousands of temporal events per second. Returning results as `IEnumerable<T>` introduces allocations (enumerators, lists, closures) and generates GC pressure.  
High-throughput consumers require deterministic memory usage, especially when match operations are invoked frequently.

## Decision
Instead of returning `IEnumerable<T>`, the library will return pooled result containers:

- `MatchPairsResult<TA, TB>`
- `MatchGroupsResult<TA, TB>`

These objects:
- rent arrays from `ArrayPool<T>`,
- expose results through `ReadOnlySpan<T>`,
- implement `IDisposable` to return arrays to the pool.

Example usage:

```csharp
using var result = TimestampMatcher.MatchPairs(...);
foreach (var pair in result.Pairs) { ... }
```

## Consequences
 - Eliminates GC overhead during match result enumeration.
 - Provides deterministic lifetime handling via Dispose().
 - Results can be processed efficiently with minimal overhead.
 - Supports high-performance IoT pipelines without memory spikes.
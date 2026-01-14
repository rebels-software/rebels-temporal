# ADR-10 — User-Provided Buffer Strategy

## Status
Accepted

## Context
IoT systems may process thousands of temporal events per second. Returning results as `IEnumerable<T>` introduces allocations (enumerators, lists, closures) and generates GC pressure.
High-throughput consumers require deterministic memory usage, especially when match operations are invoked frequently.

Traditional approaches like returning `List<T>` or using `ArrayPool<T>` internally still involve library-managed allocations or require `IDisposable` patterns that complicate API usage.

## Decision
Instead of returning allocated collections or managing pooled arrays internally, the library will require callers to provide their own result buffers.

The matching API accepts a user-provided buffer via a `ref struct`:
- Callers allocate and own the buffer (stack or heap).
- The matcher writes results directly into the provided buffer.
- The matcher returns the count of matches written.

This approach:
- Eliminates all heap allocations in the matching hot path.
- Gives callers full control over memory management.
- Supports stack allocation via `stackalloc` for small result sets.
- Avoids `IDisposable` complexity and lifetime management.

## Consequences
- Zero GC overhead during match result enumeration.
- Callers must size buffers appropriately for expected results.
- Buffer overflow is the caller's responsibility to prevent (or handle via returned count).
- Results can be processed efficiently with minimal overhead.
- Supports high-performance IoT pipelines without memory spikes.
- API is slightly more low-level but maximally flexible.

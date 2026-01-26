# INV-3 — No Allocations in Hot Path

## Rule

Core matching algorithms MUST NOT allocate heap memory during execution.

## Formal Definition

For any operation on the hot path (matching execution):

```
heap_allocations = 0
```

## Meaning

The library is designed for high-throughput, low-latency scenarios. Heap allocations during matching introduce garbage collection pressure, unpredictable pauses, and reduced throughput. All matching operations must execute without allocating memory on the managed heap.

## Implications

- Algorithms MAY assume that input and output buffers are provided by the caller.
- Algorithms MAY use stack-allocated structures and spans.
- Algorithms are NOT required to manage result storage; callers provide buffers.
- Matching throughput and latency are predictable and deterministic.

## Forbidden

- Allocating reference types (`new` for classes) in hot paths.
- Boxing value types.
- Creating closures that capture local variables.
- Using LINQ operations that allocate iterators or intermediate collections.
- Using `yield return` (state machine allocation).
- String concatenation, formatting, or interpolation in hot paths.
- Creating collections (`List<T>`, `Dictionary<T>`, etc.) during matching.

## Notes

- Allocations in error/exception paths are permitted (errors are exceptional, not hot path).
- Configuration objects created once and reused are permitted.
- Callers are responsible for providing adequately sized buffers.
- This invariant applies to all matching and relation computation logic.

## Related

- [ADR-3 — Performance Design Principles](../adr/3-performance-design-principles.md)
- [ADR-10 — User-Provided Buffer Strategy](../adr/10-user-provided-buffer-strategy.md)

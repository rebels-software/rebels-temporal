# INV-10 — Input Ordering Contract

## Rule

IF input ordering is declared (Candidates or Both), THEN the corresponding collections MUST be sorted in ascending temporal order. Behavior for unsorted input when ordering is declared is undefined.

## Formal Definition

```
IF InputOrdering = Candidates THEN candidates[i].timestamp <= candidates[i+1].timestamp ∀i
IF InputOrdering = Both THEN
    anchors[i].timestamp <= anchors[i+1].timestamp ∀i AND
    candidates[i].timestamp <= candidates[i+1].timestamp ∀i
```

For points: `timestamp = At`
For intervals: `timestamp = Start`

## Meaning

Declaring input ordering is a contract between the caller and the matching algorithm. When the caller declares that data is sorted, the algorithm MAY use optimized strategies (binary search, dual-pointer scan) that produce correct results only for sorted input. The caller takes responsibility for ensuring the ordering claim is true.

## Implications

- IF `InputOrdering = None` THEN no ordering requirement applies; any input order is valid.
- IF `InputOrdering = Candidates` THEN algorithms MAY assume candidates are sorted ascending.
- IF `InputOrdering = Both` THEN algorithms MAY assume both anchors and candidates are sorted ascending.
- Optimized algorithms MAY produce incorrect results for unsorted input.
- Behavior for unsorted input when ordering is declared is undefined and MUST NOT be relied upon.

## Forbidden

- Declaring `InputOrdering.Candidates` or `InputOrdering.Both` with unsorted data.
- Relying on specific behavior (exceptions, fallback algorithms) when the ordering contract is violated.
- Assuming the library will auto-sort input data.
- Assuming the library will detect and handle ordering violations gracefully.

## Notes

- "Sorted ascending" means non-decreasing order (equal consecutive values are permitted).
- Callers MUST ensure data is sorted before declaring ordering.
- Declaring ordering enables algorithmic optimizations but places responsibility on the caller.

## Related

- [ADR-3 — Performance Design Principles](../adr/3-performance-design-principles.md)

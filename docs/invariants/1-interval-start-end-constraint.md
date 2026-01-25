# INV-1 — Interval Start-End Constraint

## Rule

All temporal intervals MUST satisfy `Start <= End`.

## Formal Definition

For any value representing a temporal interval:

```
Start <= End
```

## Meaning

A temporal interval represents a contiguous span of time. The start of that span cannot occur after its end. Zero-duration intervals (where `Start == End`) are valid and represent instantaneous moments expressed as intervals.

## Implications

- Algorithms MAY assume that every interval has `Start <= End`.
- Algorithms are NOT required to handle or produce meaningful results for intervals where `Start > End`.
- Temporal relations (per Allen's Interval Algebra) are defined only for valid intervals.
- Behavior for invalid intervals is undefined and MUST NOT be relied upon.

## Forbidden

- Creating or passing intervals where `Start > End`.
- Interpreting `Start > End` as an inverted, wrapped, or negative-duration interval.
- Auto-correcting invalid intervals by swapping `Start` and `End`.
- Assuming that invalid intervals will be detected, rejected, or corrected automatically.

## Notes

- `Start == End` is valid (zero-duration interval).
- This invariant applies to all interval data: user-provided, generated, and test data.
- Callers and implementers MUST ensure the invariant holds.

## Related

- [ADR-6 — Temporal Relations Based on Allen's Interval Algebra](../adr/6-temporal-relations-based-on-allen-algebra.md)
- [INV-6 — Allen Relations Exhaustive](6-allen-relations-exhaustive.md)

# INV-9 — TimeTolerance Non-Negative

## Rule

Both `Before` and `After` components of a time tolerance MUST be greater than or equal to zero. Negative tolerance values are NOT permitted.

## Formal Definition

For any time tolerance:

```
Before >= 0
After >= 0
```

## Meaning

A time tolerance defines how far backward and forward from a reference timestamp matching is allowed. It represents a non-negative margin in both directions. Zero tolerance means exact matching; positive tolerance expands the matching window. Negative values have no meaningful semantic interpretation.

## Implications

- Algorithms MAY assume that tolerance values are non-negative.
- Algorithms MAY compute matching windows as `[timestamp - Before, timestamp + After]` without checking for inversion.
- The resulting window always satisfies `windowStart <= timestamp <= windowEnd`.
- Behavior for negative tolerance values is undefined and MUST NOT be relied upon.

## Forbidden

- Creating or using tolerance with negative `Before` or `After` values.
- Interpreting negative tolerance as "shrinking" or "inverting" the window.
- Auto-correcting negative values to zero or absolute values.

## Notes

- `Before = 0` and `After = 0` is valid (exact matching, no tolerance).
- Asymmetric tolerances (different `Before` and `After` values) are valid.
- Callers MUST ensure tolerance values are non-negative before use.

## Related

- [ADR-5 — Exact and Window Matchers as Core](../adr/5-exact-and-window-matchers-as-core.md)

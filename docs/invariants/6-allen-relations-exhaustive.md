# INV-6 — Allen Relations Exhaustive and Mutually Exclusive

## Rule

Any two valid temporal intervals MUST relate by exactly one of the 13 Allen relations. The relation set is exhaustive (covers all cases) and mutually exclusive (no overlap).

## Formal Definition

Given two intervals A and B where both satisfy INV-001 (`Start <= End`):

```
∃! r ∈ AllenRelations : relates(A, B) = r
```

Where `AllenRelations = { Before, Meets, Overlaps, Starts, During, Finishes, Equal, After, MetBy, OverlappedBy, StartedBy, Contains, FinishedBy }`

## Meaning

Allen's Interval Algebra defines exactly 13 possible ways two intervals can relate in time. These relations form a complete partition: every pair of valid intervals falls into exactly one relation, with no gaps and no ambiguity. This provides a mathematically sound foundation for all interval-based temporal reasoning.

## Implications

- Algorithms MAY assume that comparing two valid intervals always yields exactly one relation.
- Algorithms are NOT required to handle "unknown" or "ambiguous" relation results.
- Every interval comparison is deterministic and produces a single, unambiguous result.
- The 13 relations are sufficient to describe all possible temporal relationships.

## Forbidden

- Defining additional relations beyond the 13 Allen relations.
- Returning "none", "unknown", or multiple relations for a single interval pair.
- Treating any relation as a subset or superset of another.
- Assuming intervals can have no relation or an undefined relation.

## Notes

- This invariant depends on INV-001: both intervals MUST satisfy `Start <= End`.
- Behavior for invalid intervals is undefined and MUST NOT be relied upon (see INV-001).
- The 13 relations and their inverses: Before/After, Meets/MetBy, Overlaps/OverlappedBy, Starts/StartedBy, During/Contains, Finishes/FinishedBy, Equal (self-inverse).

## Related

- [ADR-6 — Temporal Relations Based on Allen's Interval Algebra](../adr/6-temporal-relations-based-on-allen-algebra.md)
- [INV-1 — Interval Start-End Constraint](1-interval-start-end-constraint.md)

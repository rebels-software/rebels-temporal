# INV-8 — MatchPair Relation Consistency

## Rule

A match result MUST have a temporal relation if and only if the match type is `Interval`. Point-based match types MUST NOT have a relation.

## Formal Definition

```
IF MatchType = Interval THEN Relation ≠ null
IF MatchType ∈ { PointExact, PointInInterval } THEN Relation = null
```

## Meaning

Match results carry metadata about how the match was computed. Point-based matches (exact timestamp or point-in-interval) have no interval relationship to report. Interval-based matches always produce exactly one Allen relation describing how the two intervals relate. This ensures semantic consistency between match type and relation presence.

## Implications

- Consumers MAY assume that `Relation` is present (non-null) if and only if `MatchType = Interval`.
- Consumers MAY assume that `Relation` is absent (null) for `PointExact` and `PointInInterval` matches.
- Algorithms producing match results MUST set `Relation` according to this rule.
- Behavior for invalid combinations is undefined and MUST NOT be relied upon.

## Forbidden

- Producing a `PointExact` or `PointInInterval` match with a non-null relation.
- Producing an `Interval` match without a relation.
- Interpreting missing relation on `Interval` matches as "any" or "unknown".
- Adding a "synthetic" relation to point-based matches.

## Notes

- This invariant ensures that the presence of a relation is a reliable indicator of match semantics.
- The specific relation for `Interval` matches is determined by Allen's Interval Algebra (see INV-006).

## Related

- [INV-6 — Allen Relations Exhaustive](6-allen-relations-exhaustive.md)
- [ADR-6 — Temporal Relations Based on Allen's Interval Algebra](../adr/6-temporal-relations-based-on-allen-algebra.md)

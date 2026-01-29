# ADR-17 — Tolerance Support for Interval-to-Interval Matching

## Status
Accepted

## Context
The library supports time tolerance for point-based matching operations:
- **Point-to-Point**: `AnchorTolerance` expands the anchor timestamp into a window
- **Point-to-Interval**: `AnchorTolerance` expands the anchor point before checking overlap
- **Interval-to-Point**: `CandidateTolerance` expands the candidate point

However, **Interval-to-Interval** matching currently ignores tolerance settings entirely. Allen relations are computed using exact interval boundaries (`Start`, `End`), with no flexibility for temporal proximity.

### Problem
In real-world IoT and telemetry systems, interval boundaries are rarely precise:
- Sensor sessions may have clock drift affecting start/end times
- Device state transitions have measurement uncertainty
- Business processes have fuzzy boundaries (e.g., "approximately 10:00-11:00")

Without tolerance support, users must manually expand interval boundaries before matching, which:
- Adds boilerplate code
- Breaks the consistency of the API (tolerance works for points but not intervals)
- Forces users to modify their domain data

### Proposed Solution
Apply `AnchorTolerance` to interval anchors before computing Allen relations:

| Tolerance Type | Effect on Anchor Interval |
|----------------|---------------------------|
| `ForwardOnly(X)` | `End += X` (extend forward) |
| `BackwardOnly(X)` | `Start -= X` (extend backward) |
| `Symmetric(X)` | `Start -= X`, `End += X` (extend both) |
| Asymmetric `(Before, After)` | `Start -= Before`, `End += After` |

The expanded anchor interval is then compared against candidates using Allen's Interval Algebra.

### Example

```csharp
// Anchor: [10:00, 11:00]
// Candidate: [11:00:30, 12:00]
// Without tolerance: Relation = Before (no overlap)

var policy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.ForwardOnly(TimeSpan.FromMinutes(1)),
    AllowedTemporalRelations = AllowedRelations.Meets | AllowedRelations.Overlaps
};

// With tolerance: Anchor becomes [10:00, 11:01]
// Relation = Overlaps (match!)
```

## Decision
Implement tolerance support for Interval-to-Interval matching by expanding anchor interval boundaries before computing Allen relations.

### Semantics

1. **AnchorTolerance.Before** — subtracted from `anchor.Start`
2. **AnchorTolerance.After** — added to `anchor.End`
3. **CandidateTolerance** — not applied (candidates remain exact)

This is consistent with Point-to-Interval matching where tolerance expands the anchor, not candidates.

### Algorithm

```
expandedStart = anchor.Start - AnchorTolerance.Before
expandedEnd = anchor.End + AnchorTolerance.After
relation = DetermineAllenRelation(expandedStart, expandedEnd, candidate.Start, candidate.End)
```

### Edge Cases

1. **Zero tolerance** (`TimeTolerance.None`): Behavior unchanged, exact boundaries used
2. **Large tolerance causing overlap**: Valid — may change relation from `Before` to `Overlaps`
3. **Tolerance expanding beyond candidate**: Valid — may change relation to `Contains`

## Consequences

### Positive
- Consistent API: tolerance works for all matching types
- No need for manual interval expansion in user code
- Supports real-world scenarios with imprecise boundaries
- Zero additional allocations (tolerance applied inline)

### Negative
- Slight increase in complexity of `MatchIntervalToInterval`
- Users must understand that tolerance expands anchors, not candidates
- Allen relation is computed against expanded interval, which may be counterintuitive

### Migration
- **Non-breaking change**: Default `TimeTolerance.None` preserves existing behavior
- Existing code continues to work without modification

## Related
- [ADR-5 — Exact and Window Matchers as Core](5-exact-and-window-matchers-as-core.md)
- [ADR-6 — Temporal Relations Based on Allen's Interval Algebra](6-temporal-relations-based-on-allen-algebra.md)
- [INV-3 — No Allocations in Hot Path](../invariants/3-no-allocations-in-hot-path.md)
- [INV-9 — TimeTolerance Non-Negative](../invariants/9-timetolerance-non-negative.md)

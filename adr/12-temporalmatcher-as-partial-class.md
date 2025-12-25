# ADR-12 — TemporalMatcher as Sole Public API with Partial Class Architecture

## Status
Accepted

## Context
The library needs a clear, single entry point for all temporal matching operations. Early iterations explored `TimestampMatcher` as the primary API, but the design evolved to better align with compile-time policy specialization and source generation patterns.

Key requirements:
- Single, unambiguous public API for temporal matching
- Support for four matching strategies: Point-to-Point, Point-to-Interval, Interval-to-Point, Interval-to-Interval
- Compile-time policy-based configuration via `IMatchPolicy`
- Source generator targets for partial method implementations
- Zero-allocation visitor-based operations

## Decision

### TemporalMatcher is the Sole Public API
`TemporalMatcher` is the only public class exposing temporal matching functionality. All matching operations are invoked through this class. No other matcher classes exist in the public surface area.

### Partial Class Pattern
`TemporalMatcher` is implemented as a `partial` class split across multiple files:
- **Public methods** declared in separate files for pairs and groups, containing stable API contracts
- **Partial methods** (suffixed with `Generated`) that delegate to source-generated implementations
- All parts share the same namespace (`Rebels.Temporal`)

### File Organization
```
src/Rebels.Temporal/Matching/Execution/
├── Pairs/
│   └── TemporalMatcher.cs          # Pair matching methods (IPairMatchVisitor)
└── Groups/
    └── TemporalMatcher.cs          # Group matching methods (IGroupMatchVisitor)
```

### Matching Methods

**Pair Matching (one callback per matched pair):**
```csharp
public static partial class TemporalMatcher
{
    public static void MatchPointToPoint<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    public static void MatchPointToInterval<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;

    public static void MatchIntervalToPoint<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    public static void MatchIntervalToInterval<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;
}
```

**Group Matching (one callback per anchor with all matching candidates):**
```csharp
public static partial class TemporalMatcher
{
    public static void MatchPointToPointGrouped<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    public static void MatchPointToIntervalGrouped<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;

    public static void MatchIntervalToPointGrouped<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    public static void MatchIntervalToIntervalGrouped<TAnchor, TCandidate, TPolicy>(...)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;
}
```

Each public method enforces type constraints via generic constraints and delegates to a corresponding partial method that will be source-generated.

### TimestampMatcher Removed
`TimestampMatcher` has been removed from the codebase. Since the library is pre-1.0, no migration path is required.

## Consequences

### Positive
- Clear, unambiguous API: one entry point for all matching operations
- Simplified source generator: targets single well-known class
- Better documentation: all examples use consistent API
- Type safety enforced at compile time via generic constraints
- Modular implementation via partial classes
- Single public entry point while internal complexity remains isolated

### Negative
- No fallback implementation exists; source generator must provide all implementations
- Users must use `TemporalMatcher` exclusively

### Mitigations
- Source generator emits clear diagnostics if invocations cannot be generated
- Comprehensive tests ensure `TemporalMatcher` works correctly
- Build will fail if required partial methods are not generated (by design)
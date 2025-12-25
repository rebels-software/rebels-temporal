# ADR-17 — Public-to-Generated Method Delegation Pattern

## Status
Accepted

## Context
`TemporalMatcher` is implemented as a partial class where public methods delegate to source-generated implementations. This pattern must balance several concerns:

- Public API stability: method signatures must remain constant across versions
- Source generator flexibility: generated code must adapt to different policies and types
- Type safety: compile-time constraints must be enforced before code generation
- Clear separation: public contract vs. generated implementation

Without a clear delegation pattern, it would be unclear:
- Which methods are user-facing vs. implementation details
- How to version the public API independently from generated code
- Where type constraints should be enforced
- How to handle future changes to generation logic

## Decision

### Naming Convention
Public methods delegate to partial methods with a `Generated` suffix:

```csharp
// Public API (stable, versioned)
public static void MatchPointToPoint<TAnchor, TCandidate, TPolicy>(
    ReadOnlySpan<TAnchor> anchors,
    ReadOnlySpan<TCandidate> candidates,
    IPairMatchVisitor<TAnchor, TCandidate> visitor)
    where TAnchor : ITemporalPoint
    where TCandidate : ITemporalPoint
    where TPolicy : IMatchPolicy
{
    // Delegate to generated implementation
    MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>(
        anchors, candidates, visitor);
}

// Generated implementation (internal contract)
static partial void MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>(
    ReadOnlySpan<TAnchor> anchors,
    ReadOnlySpan<TCandidate> candidates,
    IPairMatchVisitor<TAnchor, TCandidate> visitor)
    where TAnchor : ITemporalPoint
    where TCandidate : ITemporalPoint
    where TPolicy : IMatchPolicy;
```

### Responsibilities

**Public Method:**
- Declares stable API contract
- Enforces type constraints via `where` clauses
- Provides XML documentation
- Remains unchanged across versions (except for major version bumps)
- Single responsibility: validate inputs and delegate

**Generated Partial Method:**
- No accessibility modifier (effectively internal)
- Implemented by source generator
- Optimized for specific `TPolicy` configuration
- May contain different logic for different policy combinations
- Not user-visible or user-callable

### Delegation Rules
1. Public method MUST immediately delegate to corresponding `*Generated` method
2. Public method MUST NOT contain any logic beyond delegation
3. Parameter names and order MUST match exactly between public and generated methods
4. Type constraints MUST be identical between public and generated methods
5. Generated method MUST have no accessibility modifier

## Consequences

### Positive
- Clear separation of concerns: public contract vs. implementation
- Public API remains stable while generated code can evolve
- Type constraints enforced before source generation runs
- Easy to identify which methods are user-facing
- Generated methods are invisible to IntelliSense and public consumers

### Negative
- Small indirection overhead (one additional method call)
- Two method declarations per public API (duplication of signature)

### Mitigations
- Indirection is negligible and will be inlined by JIT
- Signature duplication is necessary for separation of concerns and is minimal
- Tooling can verify signature consistency in tests

### Example Usage
User calls public API:
```csharp
var policy = new ExactMatchPolicy();
TemporalMatcher.MatchPointToPoint<Anchor, Candidate, ExactMatchPolicy>(
    anchors, candidates, visitor);
```

Generated code provides implementation:
```csharp
// In TemporalMatcher.Generated.cs (source-generated file)
static partial void MatchPointToPointGenerated<Anchor, Candidate, ExactMatchPolicy>(
    ReadOnlySpan<Anchor> anchors,
    ReadOnlySpan<Candidate> candidates,
    IPairMatchVisitor<Anchor, Candidate> visitor)
{
    // Optimized implementation for ExactMatchPolicy
    for (int i = 0; i < anchors.Length; i++)
    {
        var anchor = anchors[i];
        var hasMatch = false;

        for (int j = 0; j < candidates.Length; j++)
        {
            var candidate = candidates[j];
            if (anchor.At == candidate.At) // exact match, no tolerance
            {
                visitor.OnMatch(new MatchPair<Anchor, Candidate>(
                    anchor, candidate, MatchType.PointExact));
                hasMatch = true;
            }
        }

        if (!hasMatch)
            visitor.OnMiss(anchor);
    }
}
```

## Related ADRs
- ADR-12: TemporalMatcher as sole public API with partial class architecture
- ADR-19: Per-policy source generation strategy (defines how generated methods specialize)
- ADR-9: Versioning strategy (public methods follow SemVer)

# ADR-18 — Per-Policy Source Generation Strategy

## Status
Accepted

## Context
`TemporalMatcher` uses compile-time policy configuration via the `IMatchPolicy` interface. Policies define matching behavior through static abstract members:

```csharp
public interface IMatchPolicy
{
    static abstract TimeTolerance AnchorTolerance { get; }
    static abstract TimeTolerance CandidateTolerance { get; }
    static abstract AllowedRelations AllowedTemporalRelations { get; }
    static abstract InputOrdering InputOrdering { get; }
}
```

The source generator must decide: should it generate one universal implementation that branches on policy values at runtime, or should it generate specialized implementations for each unique policy used in the codebase?

Two approaches were considered:

**Option A: Single Universal Implementation**
- Generate one `MatchPointToPointGenerated` method that checks `TPolicy` values at runtime
- Example: `if (TPolicy.InputOrdering == InputOrdering.Both) { /* fast path */ }`
- Pros: Simple generator, fewer generated files
- Cons: Runtime branching, lost optimization opportunities, defeats the purpose of compile-time policies

**Option B: Per-Policy Specialization**
- Generate separate implementations for each unique `<TAnchor, TCandidate, TPolicy>` combination
- Example: `MatchPointToPointGenerated<MyAnchor, MyCandidate, ExactPolicy>()` gets its own optimized code
- Pros: Zero runtime overhead, optimal code paths, true compile-time specialization
- Cons: More complex generator, more generated code

## Decision

### Generate Separate Implementations Per Policy
The source generator will emit a specialized implementation for **each unique combination** of `<TAnchor, TCandidate, TPolicy>` that appears in user code.

### How It Works
1. Source generator discovers all invocations of `TemporalMatcher.MatchPointToPoint<TAnchor, TCandidate, TPolicy>()`
2. For each unique `TPolicy` type:
   - Read static members from `TPolicy : IMatchPolicy`
   - Evaluate `AnchorTolerance`, `CandidateTolerance`, `AllowedRelations`, `InputOrdering` at compile time
3. Generate optimized algorithm based on policy values:
   - If `InputOrdering == InputOrdering.Both`: emit dual-pointer scan (O(n + m))
   - If `InputOrdering == InputOrdering.Candidates`: emit binary search (O(n log m))
   - If `InputOrdering == InputOrdering.None`: emit nested loops (O(n × m))
   - If `TimeTolerance.IsExact == true`: skip tolerance calculations entirely
   - If `AllowedRelations` filters specific relations: skip relation checks for excluded relations
4. Emit one partial method implementation per unique combination

### Example
Given user code:
```csharp
// User defines two policies
public struct ExactPolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.None;
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.None;
}

public struct SortedWithTolerancePolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.Symmetric(TimeSpan.FromSeconds(5));
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.Both;
}

// User calls matcher with both policies
TemporalMatcher.MatchPointToPoint<Anchor, Candidate, ExactPolicy>(
    anchors, candidates, visitor);

TemporalMatcher.MatchPointToPoint<Anchor, Candidate, SortedWithTolerancePolicy>(
    anchors, candidates, visitor);
```

Generator emits **two separate implementations**:

```csharp
// Generated file 1: ExactPolicy specialization
static partial void MatchPointToPointGenerated<Anchor, Candidate, ExactPolicy>(...)
{
    // Brute force nested loops (InputOrdering.None)
    // No tolerance checks (both tolerances are None)
    for (int i = 0; i < anchors.Length; i++)
    {
        var anchor = anchors[i];
        var hasMatch = false;

        for (int j = 0; j < candidates.Length; j++)
        {
            var candidate = candidates[j];
            if (anchor.At == candidate.At) // exact comparison only
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

// Generated file 2: SortedWithTolerancePolicy specialization
static partial void MatchPointToPointGenerated<Anchor, Candidate, SortedWithTolerancePolicy>(...)
{
    // Dual-pointer scan (InputOrdering.Both)
    // Includes anchor tolerance window calculations
    var anchorTolerance = TimeSpan.FromSeconds(5);
    int candidateIndex = 0;

    for (int i = 0; i < anchors.Length; i++)
    {
        var anchor = anchors[i];
        var hasMatch = false;
        var anchorTime = anchor.At;
        var windowStart = anchorTime - anchorTolerance;
        var windowEnd = anchorTime + anchorTolerance;

        // Advance candidate pointer to window start
        while (candidateIndex < candidates.Length &&
               candidates[candidateIndex].At < windowStart)
            candidateIndex++;

        // Scan candidates in window
        int j = candidateIndex;
        while (j < candidates.Length && candidates[j].At <= windowEnd)
        {
            visitor.OnMatch(new MatchPair<Anchor, Candidate>(
                anchor, candidates[j], MatchType.PointExact));
            hasMatch = true;
            j++;
        }

        if (!hasMatch)
            visitor.OnMiss(anchor);
    }
}
```

### Policy Uniqueness
Two policies are considered **the same** if they have identical:
- `TAnchor` type
- `TCandidate` type
- `TPolicy` type (structural equality not checked)

If user defines two different policy types with identical values, they will generate separate implementations (acceptable trade-off for simplicity).

## Consequences

### Positive
- **Zero runtime overhead**: No branching on policy values at runtime
- **Optimal code paths**: Each policy gets the most efficient algorithm for its configuration
- **True compile-time specialization**: Policy values are constants in generated code
- **Dead code elimination**: JIT can inline and optimize aggressively
- **Aligned with performance goals**: Maximizes throughput for high-volume scenarios (per ADR-3)

### Negative
- **More generated code**: N policies = N implementations per method
- **Longer build times**: Generator runs once per unique policy
- **Increased assembly size**: More IL emitted per unique policy combination

### Mitigations
- Most applications use 1-3 policies in practice, not hundreds
- Generated code is only included if actually invoked (no dead code)
- Build time impact is one-time per build, not per run
- Assembly size increase is acceptable for performance gains

### Trade-offs
We explicitly choose:
- **Runtime performance** over build-time performance
- **Per-policy specialization** over code size minimization
- **Compile-time configuration** over runtime flexibility

This aligns with ADR-3 (performance-first) and the library's focus on high-throughput event processing.

## Implementation Notes

### Generator Algorithm
1. Use `IncrementalGenerator` to track invocations
2. Group invocations by `(MethodName, TAnchor, TCandidate, TPolicy)` tuple
3. For each unique tuple:
   - Resolve `TPolicy` symbol
   - Read static property values via Roslyn semantic model
   - Emit specialized algorithm based on values
4. Output one `.g.cs` file per unique tuple (or one file with multiple partial methods)

### File Naming
Generated files should be named to avoid collisions:
```
TemporalMatcher.MatchPointToPoint_{AnchorType}_{CandidateType}_{PolicyType}.g.cs
```

Or use a single file with multiple partial methods:
```
TemporalMatcher.Generated.g.cs
```

## Related ADRs
- ADR-3: Performance design principles (justifies compile-time specialization)
- ADR-12: TemporalMatcher as sole public API
- ADR-17: Public-to-Generated delegation pattern (defines how public methods delegate to generated)

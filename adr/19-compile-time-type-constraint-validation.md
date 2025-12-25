# ADR-19 — Compile-Time Type Constraint Validation

## Status
Accepted

## Context
`TemporalMatcher` methods require specific type constraints on generic parameters:

- `MatchPointToPoint` requires `TAnchor : ITemporalPoint` and `TCandidate : ITemporalPoint`
- `MatchPointToInterval` requires `TAnchor : ITemporalPoint` and `TCandidate : ITemporalInterval`
- `MatchIntervalToPoint` requires `TAnchor : ITemporalInterval` and `TCandidate : ITemporalPoint`
- `MatchIntervalToInterval` requires `TAnchor : ITemporalInterval` and `TCandidate : ITemporalInterval`

If a user invokes a method with incorrect types (e.g., calling `MatchPointToPoint` with a type that does not implement `ITemporalPoint`), the error must be detected and reported to the developer.

Two approaches were considered:

**Option A: Runtime Validation**
- Generate code that checks type constraints at runtime
- Throw exceptions if types are invalid
- Example: `if (!(anchor is ITemporalPoint)) throw new InvalidOperationException(...)`
- Pros: Simple generator, clear error messages
- Cons: Errors discovered only when code runs, not during development or build

**Option B: Compile-Time Validation**
- Use C# generic constraints (`where` clauses) to enforce type requirements
- Source generator emits diagnostics if constraints are violated
- Build fails if invalid types are used
- Pros: Errors caught immediately during build, no runtime overhead, leverages type system
- Cons: Requires careful coordination between generic constraints and source generator diagnostics

## Decision

### Enforce Type Constraints at Compile Time
Type constraints will be enforced using **two complementary mechanisms**:

1. **Generic Constraints on Public Methods**
   - All public `TemporalMatcher` methods use `where` clauses to enforce type constraints
   - C# compiler validates constraints before source generator runs
   - Violations are caught by the compiler with standard C# error messages

2. **Source Generator Diagnostics**
   - Source generator validates that types implement required interfaces
   - If generator detects invalid types (defensive check), it emits a compile-time diagnostic
   - Build fails with clear error message indicating which type violates which constraint

### Primary Enforcement: Generic Constraints
The `where` clauses on public methods are the **primary** enforcement mechanism:

```csharp
public static void MatchPointToPoint<TAnchor, TCandidate, TPolicy>(
    ReadOnlySpan<TAnchor> anchors,
    ReadOnlySpan<TCandidate> candidates,
    IPairMatchVisitor<TAnchor, TCandidate> visitor)
    where TAnchor : ITemporalPoint      // ← Compiler enforces this
    where TCandidate : ITemporalPoint   // ← Compiler enforces this
    where TPolicy : IMatchPolicy        // ← Compiler enforces this
{
    MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>(
        anchors, candidates, visitor);
}
```

If a user writes:
```csharp
public class NotAPoint { } // Does NOT implement ITemporalPoint

TemporalMatcher.MatchPointToPoint<NotAPoint, SomeCandidate, SomePolicy>(...);
```

The **C# compiler** will emit:
```
error CS0311: The type 'NotAPoint' cannot be used as type parameter 'TAnchor'
in the generic type or method 'TemporalMatcher.MatchPointToPoint<TAnchor, TCandidate, TPolicy>(...)'.
There is no implicit reference conversion from 'NotAPoint' to 'ITemporalPoint'.
```

**No source generator logic is required for this scenario.** The compiler rejects the code before the generator runs.

### Secondary Enforcement: Generator Diagnostics
The source generator performs **defensive validation** as a safety net:

1. When generator discovers invocation of `MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>`
2. Generator validates:
   - `TAnchor` implements `ITemporalPoint`
   - `TCandidate` implements `ITemporalPoint`
   - `TPolicy` implements `IMatchPolicy`
3. If validation fails (should be impossible due to generic constraints):
   - Emit diagnostic: `REBEL_TypeConstraintViolation`
   - Severity: `Error`
   - Message: "Type '{TypeName}' does not implement required interface '{InterfaceName}'"
   - Do not generate code for this invocation

This serves as a **defense-in-depth** mechanism in case:
- Generic constraints are accidentally removed during refactoring
- Future changes introduce edge cases
- Generated code is analyzed in isolation

### Error Message Examples

**Scenario 1: User calls method with wrong type**
```csharp
public class InvalidAnchor { } // Missing ITemporalPoint

TemporalMatcher.MatchPointToPoint<InvalidAnchor, ValidCandidate, SomePolicy>(...);
```

**Result: C# compiler error (before source generator runs)**
```
error CS0311: The type 'InvalidAnchor' cannot be used as type parameter 'TAnchor'.
There is no implicit reference conversion from 'InvalidAnchor' to 'ITemporalPoint'.
```

**Scenario 2: Generic constraints are missing (hypothetical defensive case)**
If constraints were accidentally removed:
```csharp
public static void MatchPointToPoint<TAnchor, TCandidate, TPolicy>(...)
    // Missing: where TAnchor : ITemporalPoint
{
    MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>(...);
}
```

**Result: Source generator diagnostic**
```
error REBEL001: Type 'SomeType' used in MatchPointToPoint does not implement 'ITemporalPoint'.
Point-to-point matching requires both anchor and candidate types to implement ITemporalPoint.
```

## Consequences

### Positive
- **Early error detection**: Errors caught at compile time, not runtime
- **Leverages C# type system**: Standard compiler errors, familiar to developers
- **Zero runtime overhead**: No type checks in generated code
- **Clear error messages**: Compiler provides exact location and type of constraint violation
- **Prevents invalid code from compiling**: Build fails fast
- **Defense in depth**: Generator provides additional validation layer

### Negative
- **Requires generic constraints**: All public methods must have `where` clauses
- **Duplicated constraints**: Public method and generated method both have same `where` clauses

### Mitigations
- Constraint duplication is minimal and necessary for separation of concerns
- Tooling can verify constraint consistency in automated tests
- Documentation clearly explains type requirements

### Alignment with Design Philosophy
This decision aligns with:
- **ADR-3 (Performance-first)**: Zero runtime overhead from type checking
- **Fail-fast principle**: Detect errors as early as possible in development lifecycle
- **Type safety**: Use language features to enforce correctness
- **Developer experience**: Clear compiler errors better than runtime exceptions

## Implementation Notes

### C# Compiler Enforcement
The C# compiler enforces constraints **before** the source generator runs. This is standard language behavior. No special generator logic is required.

### Generator Validation (Defensive)
When generator processes an invocation:

```csharp
var anchorType = methodSymbol.TypeArguments[0];
var candidateType = methodSymbol.TypeArguments[1];
var policyType = methodSymbol.TypeArguments[2];

// Validate anchor implements ITemporalPoint (defensive check)
if (!ImplementsInterface(anchorType, temporalPointInterfaceSymbol))
{
    context.ReportDiagnostic(Diagnostic.Create(
        new DiagnosticDescriptor(
            id: "REBEL001",
            title: "Type constraint violation",
            messageFormat: "Type '{0}' does not implement 'ITemporalPoint'",
            category: "Rebels.Temporal",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true),
        location: invocationSyntax.GetLocation(),
        anchorType.ToDisplayString()));

    return; // Do not generate code
}

// Similar checks for candidateType and policyType...
```

### Diagnostic IDs
- `REBEL001`: Type does not implement ITemporalPoint
- `REBEL002`: Type does not implement ITemporalInterval
- `REBEL003`: Type does not implement IMatchPolicy
- `REBEL004`: Incorrect type combination for matching method

## Related ADRs
- ADR-12: TemporalMatcher as sole public API (defines method signatures with constraints)
- ADR-17: Public-to-Generated delegation pattern (constraints on both public and generated methods)
- ADR-3: Performance design principles (zero runtime overhead from type checks)

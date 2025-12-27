# ADR-20 — Policy Compile-Time Constant Validation

## Status
Accepted

## Context

`IMatchPolicy` defines four static abstract properties that control matching behavior:

```csharp
public interface IMatchPolicy
{
    static abstract TimeTolerance AnchorTolerance { get; }
    static abstract TimeTolerance CandidateTolerance { get; }
    static abstract AllowedRelations AllowedTemporalRelations { get; }
    static abstract InputOrdering InputOrdering { get; }
}
```

The source generator reads these property values at compile time to generate optimized matching algorithms (per ADR-18). For this approach to work correctly, **all policy property values must be compile-time constants**.

If a policy property returns a runtime-computed value, the entire premise of compile-time specialization breaks down:

**Invalid example:**
```csharp
public struct RuntimePolicy : IMatchPolicy
{
    // BAD: Reads from config file at runtime
    public static TimeTolerance AnchorTolerance =>
        GetToleranceFromConfig("anchor.tolerance");

    // BAD: Computes value dynamically
    public static InputOrdering InputOrdering =>
        Environment.ProcessorCount > 4 ? InputOrdering.Both : InputOrdering.None;
}
```

The generator would extract these values during compilation, but the actual runtime behavior would differ from what was generated, leading to incorrect matching behavior or runtime errors.

## Decision

### Enforce Compile-Time Constant Requirement

The source generator **validates that all policy property values are compile-time constants** and emits build errors if non-constant expressions are detected.

### Definition of "Compile-Time Constant"

A policy property is considered a compile-time constant if its getter returns an expression composed **exclusively** of:

1. **Constant literals** (numeric, string, enum values)
2. **Static readonly fields** with constant initializers
3. **Calls to well-known factory methods** with constant arguments:
   - `TimeSpan.FromSeconds(constant)`
   - `TimeSpan.FromMilliseconds(constant)`
   - `TimeTolerance.Symmetric(constant)`
   - `new TimeTolerance(constant, constant)`
4. **References to static properties** that are themselves constant:
   - `TimeTolerance.None`
   - `TimeSpan.Zero`
   - `AllowedRelations.Any`
5. **Bitwise operations on enum constants**:
   - `AllowedRelations.Overlaps | AllowedRelations.During`

### Valid Policy Examples

```csharp
public struct ValidExactPolicy : IMatchPolicy
{
    // Valid: static property reference
    public static TimeTolerance AnchorTolerance => TimeTolerance.None;

    // Valid: static property reference
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;

    // Valid: static property reference
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;

    // Valid: enum constant
    public static InputOrdering InputOrdering => InputOrdering.None;
}

public struct ValidTolerancePolicy : IMatchPolicy
{
    // Valid: factory method with constant argument
    public static TimeTolerance AnchorTolerance =>
        TimeTolerance.Symmetric(TimeSpan.FromSeconds(5));

    // Valid: constructor with constant arguments
    public static TimeTolerance CandidateTolerance =>
        new TimeTolerance(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200));

    // Valid: bitwise OR of enum constants
    public static AllowedRelations AllowedTemporalRelations =>
        AllowedRelations.Overlaps | AllowedRelations.During | AllowedRelations.Equal;

    // Valid: enum constant
    public static InputOrdering InputOrdering => InputOrdering.Both;
}
```

### Invalid Policy Examples

```csharp
public struct InvalidPolicy : IMatchPolicy
{
    private static TimeSpan dynamicValue = TimeSpan.FromSeconds(GetDynamicValue());

    // INVALID: References non-constant field
    public static TimeTolerance AnchorTolerance =>
        TimeTolerance.Symmetric(dynamicValue);

    // INVALID: Method call with non-constant result
    public static TimeTolerance CandidateTolerance =>
        GetToleranceFromConfiguration();

    // INVALID: Runtime conditional logic
    public static AllowedRelations AllowedTemporalRelations =>
        IsProduction() ? AllowedRelations.Equal : AllowedRelations.Any;

    // INVALID: Runtime computation
    public static InputOrdering InputOrdering =>
        Environment.ProcessorCount > 4 ? InputOrdering.Both : InputOrdering.None;
}
```

### Diagnostic Error Codes

When validation fails, the generator emits compile-time errors:

| Error Code | Description |
|------------|-------------|
| **REBEL005** | `AnchorTolerance` is not a compile-time constant |
| **REBEL006** | `CandidateTolerance` is not a compile-time constant |
| **REBEL007** | `AllowedTemporalRelations` is not a compile-time constant |
| **REBEL008** | `InputOrdering` is not a compile-time constant |
| **REBEL009** | `TimeSpan` value within tolerance is not a compile-time constant |

### Error Message Format

```
error REBEL005: Policy 'MyNamespace.MyPolicy' has non-constant AnchorTolerance.
Policy properties must be compile-time constants for source generation to work correctly.

Use TimeTolerance.None, TimeTolerance.Symmetric(TimeSpan.FromSeconds(constant)),
or new TimeTolerance(TimeSpan.FromX(constant), TimeSpan.FromY(constant)).
```

### Build Failure Behavior

If any policy property fails validation:
1. The generator emits an error diagnostic
2. The build **fails** (severity: `DiagnosticSeverity.Error`)
3. **No code is generated** for that policy
4. The user must fix the policy before the build can succeed

## Consequences

### Positive
- **Prevents runtime errors**: Invalid policies are caught at compile time, not at runtime
- **Enforces design intent**: Policies must be stable, design-time decisions (per ADR-1)
- **Clear error messages**: Developers immediately understand what's wrong and how to fix it
- **Aligns with specialization strategy**: Ensures ADR-18's per-policy optimization works correctly
- **Prevents misuse**: Users cannot accidentally create runtime-configurable policies

### Negative
- **Reduced flexibility**: Policies cannot be configured at runtime
- **Learning curve**: Developers must understand what constitutes a compile-time constant
- **Migration complexity**: Existing runtime-configured code must be refactored

### Mitigations
- Comprehensive documentation with valid/invalid examples
- Clear, actionable error messages with fix suggestions
- Future enhancement: Support multiple policies in same application, allowing runtime selection among pre-generated implementations

### Trade-offs
We explicitly choose:
- **Compile-time validation** over runtime flexibility
- **Early error detection** over permissive compilation
- **Type safety and correctness** over developer convenience

This aligns with ADR-3 (performance-first), ADR-18 (per-policy specialization), and the library's focus on high-throughput, mission-critical event processing.

## Implementation Notes

### Validation Algorithm

The generator uses Roslyn's semantic model to analyze property getters:

1. **Extract property syntax** from `IMatchPolicy` implementation
2. **Get semantic operation** for the property getter expression
3. **Recursively validate** the operation tree:
   - For `IPropertyReferenceOperation`: Check if property is known constant (e.g., `TimeTolerance.None`)
   - For `IInvocationOperation`: Check if method is factory method with constant args
   - For `IObjectCreationOperation`: Check if constructor args are constant
   - For `IFieldReferenceOperation`: Check if field is `const` or `static readonly` with constant initializer
   - For `IBinaryOperation`: Recursively validate operands
   - For literals: Always valid
4. **Report diagnostic** if any non-constant expression is found

### Roslyn Integration

The generator uses these Roslyn APIs:
- `IOperation.ConstantValue` to check for compile-time constant values
- `ISymbol` properties (`IsConst`, `IsStatic`, `IsReadOnly`) to validate field/property nature
- Semantic model to resolve symbols and analyze operation trees

### Performance Impact

Validation adds minimal overhead to source generation:
- Performed once per policy per build
- Only analyzes property getters (small syntax trees)
- Uses cached semantic models from Roslyn

## Related ADRs
- **ADR-18**: Per-policy source generation strategy (motivates constant requirement)
- **ADR-19**: Compile-time type constraint validation (establishes error code range)
- **ADR-3**: Performance design principles (justifies compile-time approach)
- **ADR-1**: Context, scope, and goals (policies as stable design decisions)

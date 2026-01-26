# Rebels.Temporal — Project Instructions for AI Assistants

## Quick Reference

| Aspect | Rule |
|--------|------|
| **Namespace** | All public types in `Rebels.Temporal` only |
| **Timestamps** | `DateTimeOffset` exclusively, never `DateTime` |
| **Intervals** | Must satisfy `Start <= End` |
| **Hot path** | Zero heap allocations |
| **Dependencies** | .NET BCL only, no NuGet packages |
| **Testing** | Chicago-style (no mocks, test behavior) |

---

## Before Making ANY Code Change

### 1. Check Applicable Invariants

All invariants are in `/docs/invariants/`. The 10 invariants are **non-negotiable**:

| ID | Name | Quick Check |
|----|------|-------------|
| INV-1 | Interval Start-End | Is `Start <= End`? |
| INV-2 | DateTimeOffset Only | No `DateTime` anywhere? |
| INV-3 | No Allocations | No `new`, LINQ, closures, boxing in hot path? |
| INV-4 | Single Namespace | Public type in `Rebels.Temporal`? |
| INV-5 | No Dependencies | No external NuGet packages? |
| INV-6 | Allen Exhaustive | Exactly one of 13 relations? |
| INV-7 | Single Pair | One anchor type + one candidate type? |
| INV-8 | Relation Consistency | Relation iff `MatchType.Interval`? |
| INV-9 | Tolerance Non-Negative | `Before >= 0` and `After >= 0`? |
| INV-10 | Ordering Contract | Sorted if declared? |

### 2. Read Relevant ADR

All ADRs are in `/docs/adr/`. Key decisions:

- **ADR-1**: Scope — temporal matching only, no I/O, no persistence
- **ADR-2**: Use `ITemporalInterval`, not "Period" in API
- **ADR-3**: Performance-first (Span, no LINQ, struct enumerators)
- **ADR-6**: Allen's Interval Algebra for all interval relations
- **ADR-10**: User-provided buffers, no internal allocations
- **ADR-12**: Single anchor-candidate pair; orchestration is caller's job
- **ADR-13**: Chicago-style testing (no mocks)

### 3. Verify Performance Constraints

Before writing hot-path code, ensure:

```csharp
// ❌ FORBIDDEN in hot path
new List<T>()              // heap allocation
items.Where(x => ...)      // LINQ allocates
items.Select(x => ...)     // LINQ allocates
foreach with class enumerator  // may allocate
string interpolation $""   // allocates
delegate/lambda capture    // allocates
boxing value types         // allocates

// ✅ ALLOWED in hot path
for (int i = 0; ...)       // no allocation
Span<T>, ReadOnlySpan<T>   // stack-based
ref struct                 // stack-only
readonly struct            // value type
stackalloc                 // stack allocation
```

---

## Code Style Guidelines

### Namespace

```csharp
// ✅ Correct
namespace Rebels.Temporal;

public readonly struct MyType { }

// ❌ Wrong
namespace Rebels.Temporal.Matching;  // sub-namespace forbidden for public types
```

### Timestamps

```csharp
// ✅ Correct
public DateTimeOffset At { get; }
public DateTimeOffset Start { get; }

// ❌ Wrong
public DateTime At { get; }          // DateTime forbidden
public DateTime? Timestamp { get; }  // DateTime forbidden
```

### Structs vs Classes

```csharp
// ✅ Prefer for domain types
public readonly struct MatchPair<T, U> { }
public readonly struct TimeTolerance { }

// ✅ OK for configuration (created once, reused)
public class MatchPolicy { }
```

### Method Signatures

```csharp
// ✅ Correct — Span-based, buffer provided by caller
public int Match<T, U>(
    ReadOnlySpan<T> anchors,
    ReadOnlySpan<U> candidates,
    MatchPolicy policy,
    ref MatchBuffer<T, U> buffer)

// ❌ Wrong — returns allocated collection
public List<MatchPair<T, U>> Match<T, U>(...)

// ❌ Wrong — uses IEnumerable (potential allocation)
public IEnumerable<MatchPair<T, U>> Match<T, U>(...)
```

---

## Testing Guidelines (Chicago-Style)

### Principles

1. **Test behavior, not implementation** — assert on results, not method calls
2. **Use real objects** — no mocks for domain types
3. **Readable test data** — use builders or factory methods
4. **One logical assertion per test** — but multiple `Assert` calls are OK if testing one behavior

### Test Structure

```csharp
[Fact]
public void PointToPoint_WithinTolerance_ReturnsMatch()
{
    // Arrange — real objects, clear setup
    var anchors = new[] { new Event(Now) };
    var candidates = new[] { new Event(Now.AddMilliseconds(50)) };
    var policy = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromMilliseconds(100))
    };
    var buffer = CreateBuffer<Event, Event>(10);

    // Act — single operation under test
    int count = MatchTemporal.Points.With.Points(anchors, candidates, policy, ref buffer);

    // Assert — verify observable outcome
    Assert.Equal(1, count);
    Assert.Equal(MatchType.PointExact, buffer.Pairs[0].MatchType);
}
```

### What NOT to Test

```csharp
// ❌ Don't test implementation details
Assert.True(matcher.InternalCacheWasUsed);

// ❌ Don't mock domain types
var mockInterval = new Mock<ITemporalInterval>();

// ❌ Don't test private methods directly
var result = matcher.GetType()
    .GetMethod("PrivateMethod", BindingFlags.NonPublic)
    .Invoke(...);
```

---

## Domain Vocabulary

| Term | Definition | Code |
|------|------------|------|
| **Anchor** | Primary event being matched | First collection in matcher |
| **Candidate** | Event matched against anchor | Second collection in matcher |
| **Point** | Single timestamp | `ITemporalPoint.At` |
| **Interval** | Time span with start/end | `ITemporalInterval.Start/End` |
| **Tolerance** | Allowed deviation window | `TimeTolerance.Before/After` |
| **Relation** | How two intervals relate | `TemporalRelation` enum |

---

## Common Tasks

### Adding a New Matcher Algorithm

1. Read ADR-3 (performance), ADR-5 (core matchers), ADR-10 (buffers)
2. Check INV-3 (no allocations), INV-7 (single pair)
3. Add method to appropriate `*AnchorsWith` struct in `MatchTemporal.cs`
4. Use `ref MatchBuffer<T, U>` for results
5. Validate inputs (ordering, intervals) at entry point
6. Write Chicago-style tests covering edge cases

### Adding a New Domain Type

1. Check INV-4 (namespace), INV-2 (DateTimeOffset)
2. Prefer `readonly struct` for value semantics
3. Implement `ITemporalPoint` or `ITemporalInterval`
4. Add XML documentation
5. Write tests with real instances

### Modifying Existing Behavior

1. Check which ADRs and invariants apply
2. Ensure backward compatibility (or document breaking change)
3. Update tests to cover modified behavior
4. Verify no new allocations in hot path

---

## File Locations

```
src/Rebels.Temporal/
├── Matching/
│   ├── Concepts/          # ITemporalPoint, ITemporalInterval, MatchType, TemporalRelation
│   ├── Policies/          # MatchPolicy, TimeTolerance, AllowedRelations, InputOrdering
│   └── Execution/         # MatchTemporal, MatchBuffer, MatchPair
docs/
├── adr/                   # Architecture Decision Records (1-13)
├── invariants/            # Non-negotiable rules (1-10)
tests/
└── Rebels.Temporal.Tests/ # Chicago-style unit tests
```

---

## Red Flags — Stop and Reconsider

If you find yourself doing any of these, **stop and reconsider**:

- Adding a `using` for a NuGet package
- Using `DateTime` instead of `DateTimeOffset`
- Creating a new namespace under `Rebels.Temporal.*`
- Adding `new List<>()` or LINQ in a matching method
- Mocking an interface in a test
- Creating an interval where `Start > End`
- Returning `IEnumerable<T>` from a matcher
- Adding infrastructure concerns (I/O, serialization, networking)

---

## Questions to Ask Yourself

Before submitting code:

1. Does this respect all 10 invariants?
2. Does this align with relevant ADRs?
3. Would this allocate memory in a hot path?
4. Is the test testing behavior or implementation?
5. Is this the simplest solution that works?

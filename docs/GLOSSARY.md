# Rebels.Temporal — Glossary

This glossary defines all domain terms used in the Rebels.Temporal library.
Each term includes a definition, code representation, and usage examples.

---

## Core Concepts

### Temporal Point

**Definition:** A single, indivisible moment in time represented by one timestamp.

**Code:** `ITemporalPoint`

```csharp
public interface ITemporalPoint
{
    DateTimeOffset At { get; }
}
```

**Examples:**
- Sensor reading at 14:30:05.123
- Button press event
- Log entry timestamp
- GPS position fix

**Implementation:**
```csharp
public readonly record struct SensorReading(
    DateTimeOffset Timestamp,
    double Value) : ITemporalPoint
{
    public DateTimeOffset At => Timestamp;
}
```

---

### Temporal Interval

**Definition:** A contiguous span of time with a defined start and end. Must satisfy `Start <= End`.

**Code:** `ITemporalInterval`

```csharp
public interface ITemporalInterval
{
    DateTimeOffset Start { get; }
    DateTimeOffset End { get; }
}
```

**Examples:**
- Device charging session (10:00 → 11:30)
- User presence in a room (09:15 → 09:45)
- Machine operating cycle
- Network connection duration

**Implementation:**
```csharp
public readonly record struct ChargingSession(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string DeviceId) : ITemporalInterval
{
    public DateTimeOffset Start => StartTime;
    public DateTimeOffset End => EndTime;
}
```

**Special case — Zero-duration interval:**
```csharp
// Valid: Start == End (instantaneous interval)
var instant = new Session(Now, Now, "device-1");
```

---

### Temporal Period (Domain Concept)

**Definition:** A business/domain concept representing something that lasts over time. This is NOT a library type — it's a domain modeling concept that maps to `ITemporalInterval`.

**Note:** The library uses "Interval" (mathematical term) instead of "Period" (domain term) to remain domain-agnostic. See [ADR-2](adr/2-intervals-vs-periods.md).

**Domain examples that map to ITemporalInterval:**
| Domain Term | Business Meaning | Maps To |
|-------------|------------------|---------|
| Charging Period | Time device was charging | `ITemporalInterval` |
| Presence Period | Time person was in location | `ITemporalInterval` |
| Maintenance Window | Scheduled downtime | `ITemporalInterval` |
| Shift | Worker's scheduled hours | `ITemporalInterval` |

---

### Time Window

**Definition:** An analytical time range derived from a reference point (anchor). Used for correlation, not as a domain concept.

**Code:** Represented by `TimeTolerance` applied to an anchor's timestamp.

```csharp
var tolerance = new TimeTolerance(
    before: TimeSpan.FromSeconds(15),
    after: TimeSpan.FromSeconds(15));

// For anchor at 10:00:00, window is [09:59:45, 10:00:15]
```

**Key distinction:**
- **Interval** = something that happened (domain fact)
- **Window** = analytical range for matching (computational construct)

**Examples:**
- "Find all events within ±30 seconds of this alarm"
- "Match telemetry received within 5 seconds after command"

---

## Matching Concepts

### Anchor

**Definition:** The primary event or interval being matched. The "reference" side of a correlation.

**Position:** First collection passed to matcher.

```csharp
// anchors = what we're trying to find matches FOR
int count = MatchTemporal.Points.With.Points(
    anchors,      // ← Anchor collection (primary)
    candidates,   // ← Candidate collection (secondary)
    policy,
    ref buffer);
```

**Mental model:** "For each anchor, find matching candidates."

---

### Candidate

**Definition:** The event or interval being tested for match against an anchor. The "search" side of correlation.

**Position:** Second collection passed to matcher.

```csharp
// candidates = what we're searching through to find matches
int count = MatchTemporal.Points.With.Points(
    anchors,      // ← Looking for matches FOR these
    candidates,   // ← Looking for matches IN these
    policy,
    ref buffer);
```

---

### Match

**Definition:** A pair of (anchor, candidate) that satisfies the matching criteria defined by the policy.

**Code:** `MatchPair<TAnchor, TCandidate>`

```csharp
public readonly struct MatchPair<TAnchor, TCandidate>
{
    public TAnchor Anchor { get; }
    public TCandidate Candidate { get; }
    public MatchType MatchType { get; }
    public TemporalRelation? Relation { get; }  // Only for Interval matches
}
```

---

### Match Type

**Definition:** Describes how a match was computed.

**Code:** `MatchType` enum

| Value | Meaning | Relation? |
|-------|---------|-----------|
| `PointExact` | Two points matched (within tolerance) | No |
| `PointInInterval` | Point matched with interval | No |
| `Interval` | Two intervals matched (Allen relation) | Yes |

```csharp
// Point-to-Point → PointExact
MatchTemporal.Points.With.Points(...) → MatchType.PointExact

// Point-to-Interval → PointInInterval
MatchTemporal.Points.With.Intervals(...) → MatchType.PointInInterval

// Interval-to-Interval → Interval
MatchTemporal.Intervals.With.Intervals(...) → MatchType.Interval
```

---

## Tolerance & Policy Concepts

### Time Tolerance

**Definition:** Defines how far before and after a reference timestamp matching is allowed.

**Code:** `TimeTolerance`

```csharp
public readonly struct TimeTolerance
{
    public TimeSpan Before { get; }  // Look backward (must be >= 0)
    public TimeSpan After { get; }   // Look forward (must be >= 0)
}
```

**Factory methods:**
```csharp
// Symmetric: same tolerance both directions
TimeTolerance.Symmetric(TimeSpan.FromSeconds(5))
// → Before=5s, After=5s, Window=10s total

// Asymmetric: different tolerances
new TimeTolerance(
    before: TimeSpan.FromSeconds(30),
    after: TimeSpan.FromSeconds(5))
// → Look 30s back, 5s forward

// Exact: no tolerance
TimeTolerance.None
// → Before=0, After=0, must match exactly
```

---

### Match Policy

**Definition:** Configuration that controls matching behavior at runtime.

**Code:** `MatchPolicy`

```csharp
public class MatchPolicy
{
    public TimeTolerance AnchorTolerance { get; set; }
    public TimeTolerance CandidateTolerance { get; set; }
    public AllowedRelations AllowedTemporalRelations { get; set; }
    public InputOrdering InputOrdering { get; set; }
}
```

**Common configurations:**
```csharp
// Exact matching, no tolerance
var exactPolicy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.None,
    InputOrdering = InputOrdering.None
};

// Window matching, ±1 second
var windowPolicy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(1)),
    InputOrdering = InputOrdering.Candidates  // O(n log m) with binary search
};

// Interval matching, only overlaps
var overlapPolicy = new MatchPolicy
{
    AllowedTemporalRelations = AllowedRelations.Overlaps | AllowedRelations.OverlappedBy
};
```

---

### Input Ordering

**Definition:** Declaration of whether input collections are pre-sorted, enabling optimized algorithms.

**Code:** `InputOrdering` enum

| Value | Meaning | Algorithm | Complexity |
|-------|---------|-----------|------------|
| `None` | No ordering guarantee | Nested loops | O(n × m) |
| `Candidates` | Candidates sorted ascending | Binary search | O(n log m) |
| `Both` | Both sorted ascending | Dual-pointer scan | O(n + m) |

**Contract:** If you declare ordering, data MUST be sorted. Undefined behavior otherwise.

```csharp
// ✅ Correct: data is sorted, declare it
var sortedAnchors = anchors.OrderBy(x => x.At).ToArray();
var sortedCandidates = candidates.OrderBy(x => x.At).ToArray();
policy.InputOrdering = InputOrdering.Both;

// ❌ Wrong: data not sorted but declared as sorted
policy.InputOrdering = InputOrdering.Both;  // UNDEFINED BEHAVIOR
```

---

## Allen's Interval Algebra

### Temporal Relation

**Definition:** One of 13 mutually exclusive ways two intervals can relate in time, per Allen's Interval Algebra.

**Code:** `TemporalRelation` enum

**The 13 Relations:**

| Relation | Diagram | Inverse |
|----------|---------|---------|
| **Before** | `A:[---]  B:      [---]` | After |
| **Meets** | `A:[---]B:[---]` | MetBy |
| **Overlaps** | `A:[---]` / `B:  [---]` | OverlappedBy |
| **Starts** | `A:[--]` / `B:[------]` | StartedBy |
| **During** | `A:  [--]` / `B:[------]` | Contains |
| **Finishes** | `A:    [--]` / `B:[------]` | FinishedBy |
| **Equal** | `A:[------]` / `B:[------]` | Equal |

**Visual reference:**
```
Before:       A[====]           B[====]
Meets:        A[====]B[====]
Overlaps:     A[====]
                 B[====]
Starts:       A[===]
              B[========]
During:          A[===]
              B[========]
Finishes:           A[===]
              B[========]
Equal:        A[========]
              B[========]
Contains:     A[========]
                 B[===]
StartedBy:    A[========]
              B[===]
FinishedBy:   A[========]
                    B[===]
OverlappedBy:    A[====]
              B[====]
MetBy:        B[====]A[====]
After:        B[====]           A[====]
```

---

### Allowed Relations

**Definition:** Bitmask specifying which Allen relations should produce matches.

**Code:** `AllowedRelations` flags enum

```csharp
[Flags]
public enum AllowedRelations
{
    None = 0,
    Before = 1 << 0,
    Meets = 1 << 1,
    // ... all 13 ...
    Any = Before | Meets | ... | FinishedBy
}
```

**Common combinations:**
```csharp
// Only overlapping intervals
AllowedRelations.Overlaps | AllowedRelations.OverlappedBy

// Intervals that touch or overlap
AllowedRelations.Meets | AllowedRelations.MetBy |
AllowedRelations.Overlaps | AllowedRelations.OverlappedBy

// Containment relationships
AllowedRelations.Contains | AllowedRelations.During

// All relations (default)
AllowedRelations.Any
```

---

## Buffer Concepts

### Match Buffer

**Definition:** User-provided storage for match results. Avoids heap allocation.

**Code:** `MatchBuffer<TAnchor, TCandidate>` (ref struct)

```csharp
public ref struct MatchBuffer<TAnchor, TCandidate>
{
    public Span<MatchPair<TAnchor, TCandidate>> Pairs;
    public int Count;
}
```

**Usage patterns:**
```csharp
// Stack allocation (small result sets)
Span<MatchPair<Event, Event>> span = stackalloc MatchPair<Event, Event>[100];
var buffer = new MatchBuffer<Event, Event> { Pairs = span };

// Heap allocation (large result sets)
var array = new MatchPair<Event, Event>[10000];
var buffer = new MatchBuffer<Event, Event> { Pairs = array };

// Reusable buffer
buffer.Count = 0;  // Reset for next use
```

---

## Summary Table

| Term | Type | Description |
|------|------|-------------|
| Temporal Point | `ITemporalPoint` | Single timestamp |
| Temporal Interval | `ITemporalInterval` | Start + End span |
| Temporal Period | (domain concept) | Business term → maps to Interval |
| Time Window | `TimeTolerance` | Analytical range for matching |
| Anchor | First collection | What we match FOR |
| Candidate | Second collection | What we match IN |
| Match | `MatchPair<T,U>` | Successful anchor-candidate pair |
| Match Type | `MatchType` | How match was computed |
| Tolerance | `TimeTolerance` | Allowed time deviation |
| Policy | `MatchPolicy` | Matching configuration |
| Input Ordering | `InputOrdering` | Sorting declaration |
| Temporal Relation | `TemporalRelation` | Allen's 13 interval relations |
| Allowed Relations | `AllowedRelations` | Filter for interval matching |
| Match Buffer | `MatchBuffer<T,U>` | User-provided result storage |

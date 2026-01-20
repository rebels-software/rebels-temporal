# Rebels.Temporal — Decision Tree

This guide helps you choose the right matcher and configuration for your use case.

---

## Quick Decision Flowchart

```
START: What are you matching?
│
├─► Both are single timestamps (points)?
│   │
│   │   Q: Do they need to match exactly?
│   │   ├─► YES → PointToPoint with TimeTolerance.None
│   │   └─► NO  → PointToPoint with TimeTolerance.Symmetric(...)
│   │
│   └─► Use: MatchTemporal.Points.With.Points(...)
│
├─► One is a timestamp, one is a time span?
│   │
│   │   Q: Which one is the primary (anchor)?
│   │   ├─► Point is anchor  → MatchTemporal.Points.With.Intervals(...)
│   │   └─► Interval is anchor → MatchTemporal.Intervals.With.Points(...)
│   │
│   └─► MatchType: PointInInterval
│
└─► Both are time spans (intervals)?
    │
    │   Q: What relationship matters?
    │   ├─► Any relationship → AllowedRelations.Any
    │   ├─► Only overlapping → AllowedRelations.Overlaps | OverlappedBy
    │   ├─► Containment → AllowedRelations.Contains | During
    │   └─► Custom → Combine flags as needed
    │
    └─► Use: MatchTemporal.Intervals.With.Intervals(...)
```

---

## Decision 1: What Matcher Do I Need?

### Identify Your Data Types

| Your Anchor | Your Candidate | Matcher | MatchType |
|-------------|----------------|---------|-----------|
| Timestamp | Timestamp | `Points.With.Points` | PointExact |
| Timestamp | Time span | `Points.With.Intervals` | PointInInterval |
| Time span | Timestamp | `Intervals.With.Points` | PointInInterval |
| Time span | Time span | `Intervals.With.Intervals` | Interval |

### Code Examples

```csharp
// CASE 1: Both are points (e.g., sensor reading ↔ command timestamp)
MatchTemporal.Points.With.Points(readings, commands, policy, ref buffer);

// CASE 2: Point anchor, interval candidate (e.g., event ↔ session it belongs to)
MatchTemporal.Points.With.Intervals(events, sessions, policy, ref buffer);

// CASE 3: Interval anchor, point candidate (e.g., session ↔ events within it)
MatchTemporal.Intervals.With.Points(sessions, events, policy, ref buffer);

// CASE 4: Both intervals (e.g., charging session ↔ usage session)
MatchTemporal.Intervals.With.Intervals(chargingSessions, usageSessions, policy, ref buffer);
```

---

## Decision 2: What Tolerance Do I Need?

### Flowchart

```
Q: Must timestamps match exactly?
│
├─► YES (same millisecond)
│   └─► TimeTolerance.None
│
└─► NO (some flexibility allowed)
    │
    Q: Same tolerance before and after?
    │
    ├─► YES (symmetric window)
    │   └─► TimeTolerance.Symmetric(TimeSpan.FromSeconds(X))
    │
    └─► NO (asymmetric window)
        │
        Q: More tolerance before or after?
        │
        ├─► More BEFORE (e.g., delayed data arrival)
        │   └─► new TimeTolerance(before: 30s, after: 5s)
        │
        └─► More AFTER (e.g., effect follows cause)
            └─► new TimeTolerance(before: 1s, after: 10s)
```

### Common Scenarios

| Scenario | Tolerance | Rationale |
|----------|-----------|-----------|
| Exact timestamp match | `TimeTolerance.None` | Timestamps must be identical |
| Clock drift compensation | `Symmetric(100ms)` | Small variance between systems |
| Delayed telemetry | `Before=30s, After=5s` | Data arrives late but not early |
| Cause → Effect | `Before=1s, After=10s` | Effect happens after cause |
| Reconnection burst | `Before=60s, After=5s` | Buffered data sent together |

### Code Examples

```csharp
// Exact matching
policy.AnchorTolerance = TimeTolerance.None;

// ±5 seconds symmetric
policy.AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(5));

// Asymmetric: 30s before, 5s after
policy.AnchorTolerance = new TimeTolerance(
    before: TimeSpan.FromSeconds(30),
    after: TimeSpan.FromSeconds(5));
```

---

## Decision 3: What Input Ordering Should I Declare?

### Flowchart

```
Q: Is your data sorted by timestamp?
│
├─► Neither collection is sorted
│   └─► InputOrdering.None (O(n×m) nested loops)
│
├─► Only candidates are sorted
│   └─► InputOrdering.Candidates (O(n log m) binary search)
│
└─► Both collections are sorted
    └─► InputOrdering.Both (O(n+m) dual pointer)
```

### Performance Comparison

| InputOrdering | Algorithm | 1K×1K | 10K×10K | 100K×100K |
|---------------|-----------|-------|---------|-----------|
| None | Nested loops | ~1ms | ~100ms | ~10s |
| Candidates | Binary search | ~0.1ms | ~1ms | ~10ms |
| Both | Dual pointer | ~0.05ms | ~0.5ms | ~5ms |

### Decision Criteria

```
Q: Can you sort your data before matching?

├─► NO (data arrives unsorted, cannot buffer)
│   └─► InputOrdering.None
│
├─► PARTIAL (can sort candidates, anchors stream in)
│   └─► InputOrdering.Candidates
│
└─► YES (can sort both before matching)
    └─► InputOrdering.Both ← PREFERRED for performance
```

### Code Examples

```csharp
// Unsorted data
policy.InputOrdering = InputOrdering.None;

// Pre-sorted candidates (e.g., from database with ORDER BY)
policy.InputOrdering = InputOrdering.Candidates;

// Both sorted (best performance)
var sortedAnchors = anchors.OrderBy(x => x.At).ToArray();
var sortedCandidates = candidates.OrderBy(x => x.At).ToArray();
policy.InputOrdering = InputOrdering.Both;
```

---

## Decision 4: Which Allen Relations Do I Need? (Interval Matching Only)

### Flowchart

```
Q: What interval relationship matters for your use case?

├─► ANY relationship (just find related intervals)
│   └─► AllowedRelations.Any
│
├─► Only OVERLAPPING intervals
│   └─► AllowedRelations.Overlaps | AllowedRelations.OverlappedBy
│
├─► One CONTAINS the other
│   └─► AllowedRelations.Contains | AllowedRelations.During
│
├─► Intervals that TOUCH (adjacent)
│   └─► AllowedRelations.Meets | AllowedRelations.MetBy
│
├─► EXACT same interval
│   └─► AllowedRelations.Equal
│
├─► BEFORE/AFTER (disjoint, ordered)
│   └─► AllowedRelations.Before | AllowedRelations.After
│
└─► CUSTOM combination
    └─► Combine flags with | operator
```

### Common Use Cases

| Use Case | Relations | Why |
|----------|-----------|-----|
| Find conflicting reservations | `Overlaps \| OverlappedBy \| Contains \| During \| Equal` | Any time overlap = conflict |
| Find parent sessions | `Contains \| StartedBy \| FinishedBy` | Anchor contains candidate |
| Find child sessions | `During \| Starts \| Finishes` | Anchor within candidate |
| Find adjacent intervals | `Meets \| MetBy` | End of one = start of other |
| Find non-overlapping | `Before \| After` | Completely separate |
| Complete temporal join | `Any` | All relationships matter |

### Visual Guide

```
Which relationships do you need?

OVERLAPPING (share time):
  Overlaps:     A[====]        OverlappedBy:    A[====]
                   B[====]                   B[====]

CONTAINMENT (one inside other):
  Contains:   A[==========]   During:         A[===]
                 B[====]                  B[==========]

TOUCHING (exactly adjacent):
  Meets:      A[====]B[====]  MetBy:      B[====]A[====]

SHARED BOUNDARY:
  Starts:     A[===]          StartedBy:  A[========]
              B[========]                 B[===]

  Finishes:        A[===]     FinishedBy: A[========]
              B[========]                      B[===]

EQUAL:
  Equal:      A[========]
              B[========]

DISJOINT (no overlap):
  Before:     A[====]     B[====]
  After:      B[====]     A[====]
```

### Code Examples

```csharp
// Any relationship (default)
policy.AllowedTemporalRelations = AllowedRelations.Any;

// Only overlapping
policy.AllowedTemporalRelations =
    AllowedRelations.Overlaps |
    AllowedRelations.OverlappedBy;

// Any kind of overlap or containment
policy.AllowedTemporalRelations =
    AllowedRelations.Overlaps |
    AllowedRelations.OverlappedBy |
    AllowedRelations.Contains |
    AllowedRelations.During |
    AllowedRelations.Starts |
    AllowedRelations.StartedBy |
    AllowedRelations.Finishes |
    AllowedRelations.FinishedBy |
    AllowedRelations.Equal;

// Containment only (anchor contains candidate)
policy.AllowedTemporalRelations =
    AllowedRelations.Contains |
    AllowedRelations.StartedBy |
    AllowedRelations.FinishedBy;
```

---

## Decision 5: How Big Should My Buffer Be?

### Estimation Guide

```
Q: How many matches do you expect?

├─► One-to-one matching (max N or M matches)
│   └─► Buffer size = max(anchors.Length, candidates.Length)
│
├─► One-to-few (each anchor matches ~K candidates)
│   └─► Buffer size = anchors.Length × K × 1.5 (safety margin)
│
├─► Many-to-many (dense matching)
│   └─► Buffer size = anchors.Length × candidates.Length (worst case)
│
└─► Unknown
    └─► Start with anchors.Length × 10, grow if needed
```

### Memory Considerations

```csharp
// Stack allocation (< 1KB recommended)
// MatchPair is ~24-48 bytes depending on types
Span<MatchPair<Event, Event>> span = stackalloc MatchPair<Event, Event>[20];

// Heap allocation (larger buffers)
var array = new MatchPair<Event, Event>[10000];

// Reusable pre-allocated buffer
private readonly MatchPair<Event, Event>[] _buffer = new MatchPair<Event, Event>[1000];
```

---

## Complete Decision Example

### Scenario: IoT Telemetry Correlation

> "Match sensor readings to device commands, allowing ±2 second tolerance.
> Both datasets come from database sorted by timestamp.
> Expect ~1000 sensors, ~500 commands, roughly 1:1 matching."

**Decisions:**

1. **Matcher:** Both are timestamps → `Points.With.Points`
2. **Tolerance:** ±2 seconds → `Symmetric(TimeSpan.FromSeconds(2))`
3. **Ordering:** Both sorted → `InputOrdering.Both`
4. **Buffer:** ~1:1 matching → `max(1000, 500) × 1.5 = 1500`

**Code:**

```csharp
var policy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(2)),
    InputOrdering = InputOrdering.Both
};

var buffer = new MatchPair<SensorReading, Command>[1500];
var matchBuffer = new MatchBuffer<SensorReading, Command> { Pairs = buffer };

int count = MatchTemporal.Points.With.Points(
    readings,   // 1000 sorted sensor readings
    commands,   // 500 sorted commands
    policy,
    ref matchBuffer);
```

---

### Scenario: Charging Session Overlap Detection

> "Find all charging sessions that overlap with usage sessions.
> Data is not sorted. Sessions number in hundreds."

**Decisions:**

1. **Matcher:** Both are intervals → `Intervals.With.Intervals`
2. **Relations:** Overlapping only → `Overlaps | OverlappedBy | Contains | During | ...`
3. **Ordering:** Not sorted → `InputOrdering.None`
4. **Buffer:** Worst case 100 × 100 = 10000

**Code:**

```csharp
var policy = new MatchPolicy
{
    AllowedTemporalRelations =
        AllowedRelations.Overlaps |
        AllowedRelations.OverlappedBy |
        AllowedRelations.Contains |
        AllowedRelations.During |
        AllowedRelations.Starts |
        AllowedRelations.StartedBy |
        AllowedRelations.Finishes |
        AllowedRelations.FinishedBy |
        AllowedRelations.Equal,
    InputOrdering = InputOrdering.None
};

var buffer = new MatchPair<ChargingSession, UsageSession>[10000];
var matchBuffer = new MatchBuffer<ChargingSession, UsageSession> { Pairs = buffer };

int count = MatchTemporal.Intervals.With.Intervals(
    chargingSessions,
    usageSessions,
    policy,
    ref matchBuffer);

// Process overlapping pairs
for (int i = 0; i < count; i++)
{
    var pair = buffer[i];
    Console.WriteLine($"{pair.Anchor.DeviceId} overlaps with {pair.Candidate.SessionId}: {pair.Relation}");
}
```

---

## Quick Reference Card

| Question | Options | Default |
|----------|---------|---------|
| What are you matching? | Points, Intervals, or mixed | — |
| Need exact timestamps? | `TimeTolerance.None` | Yes |
| Need tolerance window? | `TimeTolerance.Symmetric(X)` | — |
| Is data sorted? | `None`, `Candidates`, `Both` | `None` |
| Which relations? (intervals) | Combine `AllowedRelations` flags | `Any` |
| Buffer size? | Estimate based on expected matches | — |

---

## See Also

- [GLOSSARY.md](GLOSSARY.md) — Definitions of all terms
- [ADR-5](adr/5-exact-and-window-matchers-as-core.md) — Core matcher design
- [ADR-6](adr/6-temporal-relations-based-on-allen-algebra.md) — Allen's Interval Algebra
- [ADR-10](adr/10-user-provided-buffer-strategy.md) — Buffer strategy

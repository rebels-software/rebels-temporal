# Rebels.Temporal

[![Build](https://github.com/rebels-software/csharp-opensource-class-library-template/actions/workflows/dotnet-library-build.yml/badge.svg)](https://github.com/rebels-software/csharp-opensource-class-library-template/actions/workflows/dotnet-library-build.yml)

[![codecov](https://codecov.io/gh/rebels-software/csharp-opensource-class-library-template/graph/badge.svg?token=MJBW9OV494)](https://codecov.io/gh/rebels-software/csharp-opensource-class-library-template)

![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)

## Overview

**Rebels.Temporal** is a high-performance C# library for temporal message matching and correlation, designed especially for IoT, telemetry processing, and event-driven architectures.  
Its purpose is to provide a robust, reusable bounded context for handling all kinds of **time-based relationships**, such as event alignment, windowed matching, period analysis, and bi-temporal reasoning.

The library focuses strongly on:
- **Performance** (low allocations, span-based APIs),
- **Clear domain semantics**,
- **Flexible integration** with any existing event models,
- **Deterministic temporal logic**, common across IoT and distributed systems.

## Core Capabilities

### Exact Timestamp Matching
Match events from one or multiple collections based on the same (or nearly the same) timestamp.

Supports:
- **One-to-one pairing** (A ↔ B),
- **One-to-many grouping** (A ↔ {B₁, B₂, …}),
- Matching with optional *tolerance windows* (e.g., ±1 second).

Useful for:
- Aligning telemetry with control commands,
- Matching sensed events to logs, alarms, or measurements from another subsystem.

---

### Configurable Time-Window Matching
Define **dynamic time windows** determined by a reference event (anchor).  
For each element in a primary collection, find all events from other collections that fall within a configurable backward/forward duration.

Features include:
- Fully customizable windows (e.g., *15s backward, 15s forward*),
- Asymmetric windows (e.g., *30s backward, 5s forward*),
- Matching any number of additional event streams,
- High-performance lookup strategies for large datasets.

Common IoT scenarios:
- Linking telemetry bursts after reconnection to their original timestamps,
- Correlating sensor readings with device state changes,
- Matching access-control logs to nearby signals.

---

### Bi-Temporal and Interval Processing
Support for events that represent **durations** instead of single timestamps.

Provides mechanisms for:
- **Overlap detection** (intervals that intersect),
- **Containment checks** (intervals inside or outside other intervals),
- **Temporal decoration** (marking intervals as overlapping, contained, touching, etc.),
- **Non-destructive analysis** — periods are preserved and annotated rather than removed.

Applicable to:
- Presence tracking (e.g., "in room" intervals),
- Machine activity phases,
- Charging/discharging cycles,
- State durations derived from streaming telemetry.


## Domain Model

Rebels.Temporal defines a small, precise vocabulary for working with temporal data. Real-world domain concepts (like “charging period” or “presence interval”) are mapped onto simple, algorithm-friendly abstractions provided by the library.

| Concept           | Definition                                                                 | Description                                                                                                                             | Represented By           |
|-------------------|-------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------|---------------------------|
| **Temporal Event** | A point-in-time occurrence that has a single timestamp.                     | Used for exact matching, correlation across sources, window-based analysis, or ordering semantics.                                 | `ITemporalPint`          |
| **Temporal Period** | A real-world domain concept describing something that *lasts* from a start time to an end time. | Examples: charging session, machine running time, presence in a room, operation cycle. In domain models these carry semantics.     |  `ITemporalInterval` |
| **Time Window**     | An analytical time range centered around (or derived from) an anchor event. | Not a domain occurrence. Used purely for correlation: e.g., “±15s around event A”. Windows do not represent system states.          | `TimeWindow`              |
| **Temporal Relations** | Descriptions of how two events or intervals relate in time.                | Includes relations from interval algebra (before, after, overlaps, contains, meets, intersects). Used by matchers and analyzers.    | `TemporalRelation` |


## Getting Started

### Prerequisites
- [.NET SDK 8.0+](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### Installation

```sh
dotnet add package Rebels.Temporal
```

### Quick Start

#### Define Your Temporal Types

```csharp
using Rebels.Temporal;

// Point-in-time events
public readonly record struct SensorReading(DateTimeOffset Timestamp, double Value) : ITemporalPoint
{
    public DateTimeOffset At => Timestamp;
}

// Interval-based events
public readonly record struct DeviceSession(DateTimeOffset StartTime, DateTimeOffset EndTime, string DeviceId) : ITemporalInterval
{
    public DateTimeOffset Start => StartTime;
    public DateTimeOffset End => EndTime;
}
```

#### Match Point-to-Point Events

```csharp
// Create test data
var telemetryEvents = new[]
{
    new SensorReading(DateTimeOffset.Now, 23.5),
    new SensorReading(DateTimeOffset.Now.AddSeconds(5), 24.1),
    new SensorReading(DateTimeOffset.Now.AddSeconds(10), 23.8)
};

var commandEvents = new[]
{
    new SensorReading(DateTimeOffset.Now.AddMilliseconds(50), 0),
    new SensorReading(DateTimeOffset.Now.AddSeconds(10), 0)
};

// Configure matching policy
var policy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromMilliseconds(100)),
    InputOrdering = InputOrdering.None
};

// Allocate buffer for results
var buffer = new MatchPair<SensorReading, SensorReading>[100];
var matchBuffer = new MatchBuffer<SensorReading, SensorReading> { Pairs = buffer };

// Perform matching using fluent API
int matchCount = TemporalMatcher.Points.With.Points(
    telemetryEvents,
    commandEvents,
    policy,
    ref matchBuffer);

// Process results
for (int i = 0; i < matchCount; i++)
{
    var match = buffer[i];
    Console.WriteLine($"Matched: {match.Anchor.Value} ↔ {match.Candidate.Value} " +
                     $"(Type: {match.MatchType})");
}
```

#### Match Point-to-Interval

```csharp
var events = new[]
{
    new SensorReading(DateTimeOffset.Now, 23.5),
    new SensorReading(DateTimeOffset.Now.AddSeconds(5), 24.1)
};

var sessions = new[]
{
    new DeviceSession(DateTimeOffset.Now.AddSeconds(-1), DateTimeOffset.Now.AddSeconds(3), "Device1"),
    new DeviceSession(DateTimeOffset.Now.AddSeconds(4), DateTimeOffset.Now.AddSeconds(8), "Device2")
};

var policy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.None,
    AllowedTemporalRelations = AllowedRelations.Any
};

var buffer = new MatchPair<SensorReading, DeviceSession>[100];
var matchBuffer = new MatchBuffer<SensorReading, DeviceSession> { Pairs = buffer };

int matchCount = TemporalMatcher.Points.With.Intervals(
    events,
    sessions,
    policy,
    ref matchBuffer);
```

#### Match Interval-to-Interval with Allen Relations

```csharp
var chargingSessions = new[]
{
    new DeviceSession(DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), "Device1"),
    new DeviceSession(DateTimeOffset.Now.AddMinutes(30), DateTimeOffset.Now.AddHours(2), "Device2")
};

var usageSessions = new[]
{
    new DeviceSession(DateTimeOffset.Now.AddMinutes(15), DateTimeOffset.Now.AddMinutes(45), "Usage1"),
    new DeviceSession(DateTimeOffset.Now.AddHours(1.5), DateTimeOffset.Now.AddHours(3), "Usage2")
};

var policy = new MatchPolicy
{
    // Only match intervals that overlap or one contains the other
    AllowedTemporalRelations = AllowedRelations.Overlaps |
                              AllowedRelations.OverlappedBy |
                              AllowedRelations.Contains |
                              AllowedRelations.During
};

var buffer = new MatchPair<DeviceSession, DeviceSession>[100];
var matchBuffer = new MatchBuffer<DeviceSession, DeviceSession> { Pairs = buffer };

int matchCount = TemporalMatcher.Intervals.With.Intervals(
    chargingSessions,
    usageSessions,
    policy,
    ref matchBuffer);

// Access Allen relation for each match
for (int i = 0; i < matchCount; i++)
{
    var match = buffer[i];
    Console.WriteLine($"Interval relation: {match.Relation}");
}
```

#### Performance Optimization with Sorted Data

```csharp
// If your data is pre-sorted, use InputOrdering for optimized algorithms
var sortedPolicy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(1)),
    InputOrdering = InputOrdering.Both  // O(n+m) dual-pointer scan
};

// Or just candidates sorted
var candidatesSortedPolicy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(1)),
    InputOrdering = InputOrdering.Candidates  // O(n log m) binary search
};
```

## Working with AI Assistants

This repository is optimized for contributing with help from modern LLM-based assistants (ChatGPT, Claude, Mistral, Gemini, etc.).

If you are a new contributor and want your AI model to fully understand the project, please follow these steps:

1. **Load the repository** into your AI model so it can read the codebase and documentation.
2. **Copy and paste the initialization prompt below** into your AI assistant.
3. **Tip:** Most AI assistants will perform better if you explicitly ask them to *read all files first* before answering any question.

### AI Initialization Prompt

```text
You are assisting as a contributor to the open-source library Rebels.Temporal.

Load and study the following repository structure, including its documentation and architecture decision records:
- README.md
- /docs (all files)
- /adr (all Architecture Decision Records)
- /Domain and its subfolders
- /Engine and all public API types

Your goals:
1. Understand the temporal domain model used by the library, including:
   - Temporal Events
   - Temporal Periods vs Temporal Intervals
   - Time Windows
   - Temporal Relations
2. Understand the design philosophy, performance principles, and boundaries of the project.
3. Respect all decisions declared in ADRs.
4. Provide answers and code suggestions consistent with the existing architecture.
5. When asked about new features, propose solutions aligned with the project’s domain model and design constraints.

After loading all documents, acknowledge with:
"Rebels.Temporal context loaded and understood. Ready to contribute."
```

Detailed usage examples and API documentation will be available soon in the /docs directory and GitHub Wiki.

## Architecture Decision Records (ADR)

All architectural decisions for this project are documented in the `/adr` directory.

If you contribute to this library, please read the ADRs before making changes,  
and propose new ADRs for any significant decisions.


## Contributing
We welcome contributions! Please follow these steps:
  1. Fork this repository.
  2. Create a new branch (git checkout -b feature-name).
  3. Commit your changes (git commit -m "Add feature").
  4. Push to your branch (git push origin feature-name).
  5. Open a Pull Request. ### Code Style Ensure code follows the .NET coding standards: 
      - Use dotnet format to auto-format code. 
      - Run dotnet test before submitting a PR. 

## License
 This project is licensed under the [Apache 2.0 License](LICENSE). ## Contact For questions or support, open an issue or contact us at [we@rebels.software](mailto:we@rebels.software).
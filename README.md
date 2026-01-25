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
| **Temporal Event** | A point-in-time occurrence that has a single timestamp.                     | Used for exact matching, correlation across sources, window-based analysis, or ordering semantics.                                 | `ITemporalPoint`          |
| **Temporal Period** | A real-world domain concept describing something that *lasts* from a start time to an end time. | Examples: charging session, machine running time, presence in a room, operation cycle. In domain models these carry semantics.     |  `ITemporalInterval` |
| **Time Window**     | An analytical time range centered around (or derived from) an anchor event. | Not a domain occurrence. Used purely for correlation: e.g., “±15s around event A”. Windows do not represent system states.          | `TimeWindow`              |
| **Temporal Relations** | Descriptions of how two events or intervals relate in time.                | Includes relations from interval algebra (before, after, overlaps, contains, meets, intersects). Used by matchers and analyzers.    | `TemporalRelation` |


## Where This Library Fits

Rebels.Temporal is designed to be used in the **application layer** of your solution, bridging infrastructure (where events arrive) and domain logic (where business rules apply):

```
┌─────────────────────────────────────────────────────────────┐
│                      YOUR SOLUTION                          │
├─────────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                       │
│  (Kafka, Azure IoT Hub, MQTT, databases)                    │
│  └─► Receives raw events, deserializes data                 │
├─────────────────────────────────────────────────────────────┤
│  Application Layer                                          │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  Rebels.Temporal                                       │ │
│  │  └─► Correlates events temporally                      │ │
│  │  └─► Finds matches based on time windows               │ │
│  └────────────────────────────────────────────────────────┘ │
│  └─► Orchestrates matching across sources                   │
│  └─► Prepares correlated data for domain processing         │
├─────────────────────────────────────────────────────────────┤
│  Domain Layer                                               │
│  └─► Applies business rules to matched events               │
│  └─► Makes decisions based on correlations                  │
│  └─► Contains your domain-specific logic                    │
└─────────────────────────────────────────────────────────────┘
```

### The library does NOT:
- Connect to message brokers, IoT platforms, or databases
- Deserialize or transform your event data
- Apply business rules to matches
- Manage event streams or subscriptions
- Provide integration packages for specific platforms

### The library DOES:
- Provide high-performance temporal matching algorithms
- Define clear temporal semantics (Allen's Interval Algebra)
- Give you full control over memory and performance
- Work with any event types that implement `ITemporalPoint` or `ITemporalInterval`

This separation ensures that Rebels.Temporal remains focused, testable, and free of external dependencies, while your solution retains full control over infrastructure and business logic.


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
int matchCount = MatchTemporal.Points.With.Points(
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

int matchCount = MatchTemporal.Points.With.Intervals(
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

int matchCount = MatchTemporal.Intervals.With.Intervals(
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

The `InputOrdering` setting has a **dramatic impact** on performance. Always prefer sorted data when possible.

##### Benchmark Results (2,000 anchors × 2,000 candidates)

| InputOrdering | Algorithm | Time | Complexity |
|---------------|-----------|------|------------|
| `Both` | Dual-pointer scan | **56 μs** | O(n+m) |
| `None` | Nested loops | **14.4 ms** | O(n×m) |

**Sorted data is ~255x faster.** For larger datasets the difference grows exponentially:

| Dataset Size | Sorted O(n+m) | Unsorted O(n×m) |
|--------------|---------------|-----------------|
| 2k × 2k | 56 μs | 14 ms |
| 100k × 100k | ~3 ms | ~46 seconds |

##### Recommendation

```csharp
// RECOMMENDED: Pre-sort your data for best performance
var sortedAnchors = anchors.OrderBy(x => x.At).ToArray();
var sortedCandidates = candidates.OrderBy(x => x.At).ToArray();

var policy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(1)),
    InputOrdering = InputOrdering.Both  // O(n+m) dual-pointer scan
};

// If only candidates come from a sorted source (e.g., database with ORDER BY)
var candidatesSortedPolicy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(1)),
    InputOrdering = InputOrdering.Candidates  // O(n log m) binary search
};

// Use None only when sorting is impossible or data is very small
var unsortedPolicy = new MatchPolicy
{
    InputOrdering = InputOrdering.None  // O(n×m) - avoid for large datasets
};
```

> **Note:** The cost of sorting (`OrderBy`) is O(n log n), which is negligible compared to the gains from O(n+m) matching.

## Working with AI Assistants

This repository is optimized for contributing with help from modern LLM-based assistants (ChatGPT, Claude, Mistral, Gemini, etc.).

### Initialization

Use the `/init` command to load the full project context into your AI assistant.

The command will load:
- Repository structure and documentation
- Architecture Decision Records (ADRs)
- System invariants
- Source code with domain model

After initialization, the AI will confirm with:
```
Rebels.Temporal context loaded and understood. Ready to contribute.
```

### Available Commands

| Command | Description |
|---------|-------------|
| `/init` | Initialize LLM context — loads all documentation and code |
| `/why`  | Explain design decisions — answers "why" questions about architecture |
| `/benchmark` | Run performance benchmarks — measure and compare implementations |

Full command documentation: [docs/COMMANDS.md](docs/COMMANDS.md)

### Tips

- Always run `/init` at the start of a new session
- Use `/why` when you want to understand design decisions
- AI assistants perform better when they read all files before answering

Detailed usage examples and API documentation are available in the [/docs](docs/) directory.

## Architecture Decision Records (ADR)

All architectural decisions for this project are documented in the `/docs/adr` directory.

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

 temp:
 Tak napisales o projekcie:
 ✅ MOCNE STRONY (Co jest bardzo dobre)

  1. Doskonała dyscyplina architektoniczna

  - ADRs (Architecture Decision Records) - To jest wzorcowe podejście. 13 ADRów dokumentuje każdą istotną decyzję. To rzadkość w projektach open source.
  - Invariants - Explicite zapisane niezmienniki systemu to coś, czego brakuje w 99% projektów. To sprawia, że każdy contributor wie, czego NIE wolno zrobić.
  - Consistency - Widać, że projekt ma jednego architekta lub bardzo zgrany zespół. Wszystko jest spójne.

  2. Rzeczywista optymalizacja wydajności

  // O(n×m) → O(n+m) to 255x przyśpieszenie
  InputOrdering.None:  14.4 ms  // nested loops
  InputOrdering.Both:  56 μs    // dual-pointer scan

  To NIE jest premature optimization - to faktycznie rozwiązuje realny problem IoT:
  - Zero alokacji w hot path = brak GC pauses
  - ReadOnlySpan<T> = brak kopiowania danych
  - User-provided buffers = pełna kontrola nad pamięcią

  Problem: W IoT z 10,000 sensorów wysyłających dane co sekundę, każda milisekunda GC pause = stracone eventy.

  3. Allen's Interval Algebra - solidne fundamenty matematyczne

  Użycie algebry Allen'a to profesjonalne podejście. To nie jest "wymyślone od nowa", tylko sprawdzona teoria z 1983 roku. Implementacja w DetermineAllenRelation() (linie 461-493) wygląda poprawnie.

  4. Fluent API jest eleganckie

  MatchTemporal.Points.With.Points(...)
  MatchTemporal.Intervals.With.Intervals(...)

  Czytelne, type-safe, odkrywalne przez IntelliSense. Dobry design.

  ---
  ⚠️ OBSZARY DO POPRAWY (Co można zrobić lepiej)

  1. API jest zbyt niskopoziomowe dla przeciętnego developera

  Problem:
  // Trzeba ręcznie alokować buffer, przekazywać przez ref, sprawdzać count...
  var buffer = new MatchPair<SensorReading, CommandEvent>[100];
  var matchBuffer = new MatchBuffer<SensorReading, CommandEvent> { Pairs = buffer };
  int matchCount = MatchTemporal.Points.With.Points(
      anchors, candidates, policy, ref matchBuffer);

  for (int i = 0; i < matchCount; i++) {  // ręczna iteracja
      var match = buffer[i];
  }

  Co większość developerów chce:
  var matches = MatchTemporal.Points.With.Points(anchors, candidates, policy);
  foreach (var match in matches) { ... }

  Rozwiązanie: Dodaj convenience layer:
  // High-performance path (obecny)
  public static int Points<TAnchor, TCandidate>(
      ReadOnlySpan<TAnchor> anchors,
      ReadOnlySpan<TCandidate> candidates,
      MatchPolicy policy,
      ref MatchBuffer<TAnchor, TCandidate> buffer)

  // Convenience path (NOWY - dla 80% use cases)
  public static IEnumerable<MatchPair<TAnchor, TCandidate>> Points<TAnchor, TCandidate>(
      IEnumerable<TAnchor> anchors,
      IEnumerable<TCandidate> candidates,
      MatchPolicy policy)
  {
      // Internally użyj pooled buffer, ToArray, etc.
  }

  Opcja: Możesz użyć ArrayPool<T> dla convenience API, żeby dalej minimalizować alokacje.

  2. Brak wsparcia dla streaming/real-time scenarios

  IoT to często streaming data, nie batch processing. Obecny API wymaga:
  1. Mieć WSZYSTKIE anchory w pamięci
  2. Mieć WSZYSTKIE candidates w pamięci
  3. Dopiero wtedy matchować

  Realny IoT scenario:
  - Events przychodzą ze stream (Kafka, Azure IoT Hub, MQTT)
  - Nie możesz czekać aż "zbierze się batch"
  - Chcesz matchować "on the fly"

  Propozycja:
  // Streaming matcher - utrzymuje sliding window
  public class StreamingMatcher<TAnchor, TCandidate>
      where TAnchor : ITemporalPoint
      where TCandidate : ITemporalPoint
  {
      private readonly CircularBuffer<TAnchor> _anchorWindow;
      private readonly CircularBuffer<TCandidate> _candidateWindow;

      public void AddAnchor(TAnchor anchor) { }
      public void AddCandidate(TCandidate candidate) { }

      // Zwraca matches dla events w sliding window
      public void GetMatches(ref MatchBuffer<TAnchor, TCandidate> buffer) { }
  }

  To byłoby game changer dla real-time IoT.

  3. Brak obsługi late arrivals / out-of-order events

  W distributed IoT systemach events arrive out of order:
  - Sensor stracił połączenie, buforował lokalne, potem wysłał wszysko
  - Network latency z różnych regionów
  - Różne clock skew między urządzeniami

  Problem: Twoje InputOrdering.Both założenie (sorted data) łamie się w realnym świecie.

  Propozycja: Dodaj watermarking mechanism:
  public class WatermarkedMatcher<TAnchor, TCandidate>
  {
      public void SetWatermark(DateTimeOffset watermark);  // "no events before this time will arrive"
      public void ProcessEvent(TEvent evt);
      public void GetCompletedMatches(ref MatchBuffer buffer);  // matches where all data arrived
  }

  4. Brak metryk i observability

  W production IoT systemach potrzebujesz:
  - Ile events zostało zmatchowanych?
  - Ile events nie znalazło pary?
  - Jaka jest średnia latencja matchingu?
  - Ile events jest "too old" lub "too new"?

  Propozycja:
  public class MatchMetrics
  {
      public int TotalAnchors { get; set; }
      public int TotalCandidates { get; set; }
      public int MatchedPairs { get; set; }
      public int UnmatchedAnchors => TotalAnchors - MatchedPairs;
      public TimeSpan ProcessingTime { get; set; }
  }

  // W API:
  public static int Points<TAnchor, TCandidate>(
      ...,
      out MatchMetrics metrics)

  5. Tolerancje są symetryczne w czasie, ale nie w semantyce

  TimeTolerance.Symmetric(TimeSpan.FromSeconds(5))  // ±5s

  Problem: W IoT często masz asymmetric causality:
  - Command wysłany o 10:00:00
  - Sensor response może przyjść 10:00:00 do 10:00:05 (latencja)
  - Ale sensor response NIE MOŻE przyjść przed command (causality violation)

  Propozycja: Rozszerz semantykę:
  public enum ToleranceSemantics
  {
      Symmetric,      // ±tolerance (obecne)
      CausalForward,  // 0 before, +tolerance after (command → response)
      CausalBackward  // -tolerance before, 0 after (response → command lookup)
  }

  6. Brak integracji z popularnych IoT platforms

  ADR-7 mówi "no external dependencies", ale to sprawia, że projekt jest trudny do użycia w realnych scenariuszach.

  Propozycja: Stwórz osobne projekty (w stylu Serilog):
  - Rebels.Temporal - core (zero dependencies) ✅
  - Rebels.Temporal.Azure - integracja z Azure IoT Hub, Event Hubs
  - Rebels.Temporal.Kafka - integracja z Kafka Streams
  - Rebels.Temporal.Mqtt - integracja z MQTT brokers

  To nie łamie ADR-7, bo core pozostaje pure.

  7. TimeTolerance przechowuje TimeSpan, ale w IoT często masz clock drift

  W distributed IoT systemach masz clock skew między urządzeniami. Dwa sensory mogą mieć różnicę np. 200ms w zegarach.

  Problem: Twój TimeTolerance nie rozróżnia:
  - "events within 1 second" (user intent)
  - "clocks may be off by 500ms" (system reality)

  Propozycja:
  public class MatchPolicy
  {
      public TimeTolerance AnchorTolerance { get; set; }
      public TimeSpan ClockSkewTolerance { get; set; }  // NOWE - dodatkowa tolerancja na clock drift
  }

  8. Brak wsparcia dla "confidence scoring"

  W IoT często matchujesz niepewne dane. Przykład:
  - Anchor: 10:00:00
  - Candidate A: 10:00:00.001 (bardzo bliski)
  - Candidate B: 10:00:01.500 (w tolerancji ±2s, ale daleki)

  Oba są valid matches, ale A jest lepszy. Obecne API nie daje tego znać.

  Propozycja:
  public readonly struct MatchPair<TAnchor, TCandidate>
  {
      public TAnchor Anchor { get; }
      public TCandidate Candidate { get; }
      public MatchType MatchType { get; }
      public TemporalRelation? Relation { get; }

      public double ConfidenceScore { get; }  // NOWE: 0.0-1.0, based on time delta
  }

  ---
  🎯 OCENA KOŃCOWA

  Jako Senior Developer: ⭐⭐⭐⭐½ (4.5/5)

  Plusy:
  - Kod bardzo czysty, SOLID principles
  - Doskonała dokumentacja (XML comments, ADRs)
  - Performance-aware bez przedwczesnej optymalizacji
  - Testy (Chicago School approach) są właściwe

  Minusy:
  - API zbyt niskopoziomowe - brakuje convenience layer
  - Zero abstrakcji dla częstych use cases

  Jako Architekt: ⭐⭐⭐⭐ (4/5)

  Plusy:
  - ADRs i Invariants to wzór do naśladowania
  - Clear boundaries (pure domain library)
  - Excellent separation of concerns
  - Allen's Algebra to solidny fundament matematyczny

  Minusy:
  - Za sztywny "no dependencies" dogmat - utrudnia adopcję
  - Brak extension points dla custom matching strategies
  - No plugin architecture dla telemetry/metrics

  Jako Znawca IoT: ⭐⭐⭐ (3/5)

  Plusy:
  - Zero-allocation design jest kluczowy dla edge devices
  - Sorted data optimization (O(n+m)) rozwiązuje real problem
  - DateTimeOffset-only chroni przed timezone bugs

  Minusy:
  - Brak streaming support - to największy problem
  - Brak obsługi late arrivals / out-of-order - krityczny w distributed IoT
  - Brak watermarking - jak wiesz, że "dane są kompletne"?
  - Brak clock skew handling - każdy distributed system ma clock drift
  - Batch-only mindset - IoT to streaming, nie batch processing
  - Brak integracji z Azure IoT Hub, AWS IoT Core, MQTT, Kafka

  ---
  📋 REKOMENDACJE

  Krótkoterminowe (Quick Wins):

  1. Dodaj convenience API - wrapper z IEnumerable<T> dla 80% use cases
  2. Dodaj metrics/observability - MatchMetrics struct
  3. Confidence scoring - ranked matches

  Średnioterminowe:

  4. Streaming support - StreamingMatcher z sliding window
  5. Late arrival handling - watermarking mechanism
  6. Integration packages - Rebels.Temporal.Azure, .Kafka, .Mqtt

  Długoterminowe:

  7. Complex Event Processing (CEP) - patterns like "A followed by B within 5 seconds"
  8. State machine matching - dla workflow scenarios
  9. Distributed matching - partitioned matching across nodes

  ---
  💭 CZY JEST TO UŻYTECZNE?

  TAK, ale z zastrzeżeniami:

  ✅ Użyteczne dla:
  - Batch processing telemetry (post-facto analysis)
  - Small-scale IoT (< 1000 devices)
  - Edge devices z ograniczoną pamięcią
  - Scenarios gdzie masz WSZYSTKIE dane przed matchingiem

  ❌ Mniej użyteczne dla:
  - Large-scale streaming IoT (10,000+ devices)
  - Real-time alerting systems
  - Distributed event processing
  - Systems z high out-of-order arrival rate

  ---
  🚀 CZY MOŻNA LEPIEJ?

  TAK, ale to ewolucja, nie rewolucja:

  Projekt ma świetne fundamenty. Nie trzeba wszystkiego przepisywać. Proponuję:

  1. Zachowaj core (obecny design) - to jest solidne
  2. Dodaj streaming layer - nowa funkcjonalność
  3. Stwórz ecosystem - integration packages
  4. Rozbuduj o CEP - complex event processing

  Analogia: To jak Redis:
  - Redis Core - prosty, szybki, zero dependencies
  - Redis Modules - streaming, AI, JSON, etc.
  - Rebels.Temporal może być podobnie

  ---
  📊 BENCHMARK SUGGESTION

  Zrób porównanie z konkurencją:
  - NodaTime - ma interval matching?
  - Rx (Reactive Extensions) - time-based operators
  - Custom solutions - co ludzie piszą teraz?

  Pokaż, że Twoje podejście jest measurably better.

  ---
  TL;DR: Projekt ma świetny core, doskonałą architekturę i dokumentację, ale jest zbyt academic/theoretical. Brakuje mu pragmatic features dla real-world IoT streaming scenarios. Dodanie streaming support i integration packages uczyniłoby go production-ready.

Spiszmy jako todolist gdzieś w głównym readme.md: convenience layer, benchmarks z nodatime,rx, confidence scoring, matchmetrics, complex event processing, streaming support, rozbudowac ecosystem (integration packages). Napisz w kilku          
  zdaniach co kazdy z tych etapow ma robic. Pozniej porozmawiamy o kazdym.  

Jako drugi krok chce bys spisal jako osobny plik, np doubts.md : no plugin architecture, brak extension points, za sztywny dogmat no dependencies, brak obslugi late arrivals i out of order, brak watermarking, brak clock skew, batch only mindset, integracja z iothub, core, mqtt. Je bede chcial omowic, bo moze brakuje czegos w ADR i celach biblioteki by to bylo jasne od poczatku
# ADR-16 — Streaming and Late Arrivals Out of Scope

## Status
Accepted

## Context
In distributed IoT and event-driven systems, events often arrive out of order:
- Sensors lose connectivity, buffer locally, then send batched data on reconnect
- Network latency varies across regions
- Clock skew between devices affects ordering

Related streaming concepts include:
- **Watermarking** — declaring "no events with timestamp < X will arrive" to finalize processing
- **Late arrival policies** — what to do when events arrive after their watermark (ignore, reprocess, buffer)
- **Sliding windows** — maintaining a time-based buffer of recent events for continuous matching

These are well-understood problems in stream processing, addressed by frameworks like Apache Kafka, Apache Flink, Spark Streaming, and Azure Stream Analytics.

### The Question
Should Rebels.Temporal provide streaming capabilities, watermark tracking, or late arrival handling?

## Decision
**No.** Streaming, watermarking, and late arrival handling are explicitly **out of scope** for Rebels.Temporal.

### Rationale

1. **The library assumes data is ready.**
   Rebels.Temporal is designed as a batch matching utility. When you call the matching API, you are asserting: "Here are my anchors and candidates — match them now."

2. **Library scope is temporal matching logic, not data collection.**
   The library helps with one small step in the processing pipeline: the temporal correlation logic. It does not manage event ingestion, buffering, or reprocessing.

3. **Streaming is an infrastructure concern.**
   Per ADR-1 (Architectural Position), infrastructure concerns belong to the consumer's infrastructure layer:
   ```
   ┌─────────────────────────────────────────────────────────────┐
   │  Infrastructure Layer  ◄── Streaming, watermarking here    │
   │  (Kafka, Flink, Event Hubs, custom pipelines)              │
   ├─────────────────────────────────────────────────────────────┤
   │  Application Layer  ◄── Rebels.Temporal here               │
   │  (temporal matching on ready data)                         │
   ├─────────────────────────────────────────────────────────────┤
   │  Domain Layer                                               │
   └─────────────────────────────────────────────────────────────┘
   ```

4. **Streaming frameworks already solve this.**
   Kafka, Flink, Spark Streaming, and Azure Stream Analytics provide mature, battle-tested solutions for watermarking and late arrivals. Duplicating this functionality would be redundant and inferior.

5. **Adding streaming would violate core principles.**
   - ADR-7 (No External Dependencies) — streaming often requires external libraries
   - INV-3 (No Allocations in Hot Path) — streaming buffers require allocations
   - ADR-4 (No Infrastructure Layer) — streaming is infrastructure

## Usage Patterns

### Batch Processing (historical analysis)
```csharp
// Load data from storage
var anchors = await db.LoadSensorReadings(startTime, endTime);
var candidates = await db.LoadCommands(startTime, endTime);

// Data is ready — match
MatchTemporal.Points.With.Points(anchors, candidates, policy, ref buffer);
```

### Warm Processing (near real-time)
```csharp
// Periodically pull recent data
var anchors = await cache.GetRecentReadings(TimeSpan.FromMinutes(5));
var candidates = await cache.GetRecentCommands(TimeSpan.FromMinutes(5));

// Data is ready — match
MatchTemporal.Points.With.Points(anchors, candidates, policy, ref buffer);
```

### Stream Processing (with external framework)
```csharp
// Kafka/Flink handles windowing, watermarks, late arrivals
stream
    .Window(TumblingWindow.Of(TimeSpan.FromMinutes(1)))
    .Apply(window =>
    {
        var anchors = window.Anchors.ToArray();
        var candidates = window.Candidates.ToArray();

        // Data is ready (window closed) — match
        MatchTemporal.Points.With.Points(anchors, candidates, policy, ref buffer);

        return ProcessMatches(buffer);
    });
```

## Consequences
- The library remains focused and simple.
- Consumers choose their own streaming/batching strategy.
- No infrastructure dependencies are introduced.
- The library works equally well in batch, warm, and stream processing pipelines.
- Consumers using streaming frameworks can integrate Rebels.Temporal as a windowed operation.

## What This Means for Consumers

| If you need... | Use... |
|----------------|--------|
| Batch matching on historical data | Rebels.Temporal directly |
| Continuous stream processing | Kafka Streams, Flink, etc. + Rebels.Temporal in window functions |
| Watermarking / late arrival handling | Your streaming framework's native capabilities |
| Sliding window matching | Your streaming framework for windowing + Rebels.Temporal for matching |

## Related
- [ADR-1 — Context, Scope, and Goals](1-context-scope-and-goals.md)
- [ADR-4 — No Application and Infrastructure Layers](4-no-application-and-infrastructure.md)
- [ADR-7 — No External Dependencies](7-no-external-dependencies.md)

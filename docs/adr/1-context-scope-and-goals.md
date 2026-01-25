# ADR-1 – Context, Scope, and Goals of Rebels.Temporal

## Status
Accepted

## Context
IoT systems and event-driven architectures frequently require matching, correlating, and analyzing large volumes of time-based data originating from multiple sources.
Typical implementations solve these issues repeatedly in custom ways, often inefficiently and inconsistently.
Rebels.Temporal aims to provide a reusable, high-performance bounded context focused exclusively on temporal reasoning.

The library is not an IoT platform, not a workflow engine, and not an analytics framework.
It provides only a domain model and algorithms necessary for matching events, analyzing intervals, and constructing time windows.

## Decision
Create a stand-alone, self-contained library whose sole responsibility is temporal event and interval processing.
The project will expose a clean domain model, high-performance matchers, and deterministic algorithms while intentionally avoiding concerns such as persistence, messaging, UI, networking, or application-layer orchestration.

## Architectural Position

Rebels.Temporal is designed to operate in the **application layer** of consuming solutions:

```
┌─────────────────────────────────────────────────────────┐
│                 CONSUMER'S SOLUTION                     │
├─────────────────────────────────────────────────────────┤
│  Infrastructure Layer                                   │
│  - Message brokers (Kafka, MQTT, Azure IoT Hub)        │
│  - Databases and persistence                            │
│  - Network communication                                │
├─────────────────────────────────────────────────────────┤
│  Application Layer  ◄── Rebels.Temporal operates here  │
│  - Event deserialization and preparation                │
│  - Temporal matching and correlation                    │
│  - Orchestration of multiple match operations           │
├─────────────────────────────────────────────────────────┤
│  Domain Layer                                           │
│  - Business rules applied to correlated events          │
│  - Domain-specific decisions and logic                  │
└─────────────────────────────────────────────────────────┘
```

### What this means:

1. **Infrastructure is the consumer's responsibility.**
   The library does not provide connectors, adapters, or integration packages for specific platforms. Consumers bring their own infrastructure and feed data to Rebels.Temporal.

2. **Domain logic is the consumer's responsibility.**
   The library correlates events temporally but does not interpret what those correlations mean for the business. Consumers apply their own business rules to match results.

3. **The library focuses purely on temporal reasoning.**
   It provides the algorithms, data structures, and semantics needed to answer questions like "which events occurred together?" or "how do these intervals relate?"

### Explicit non-goals:

- Platform-specific integration packages (e.g., `Rebels.Temporal.Azure`)
- Event deserialization or data transformation
- Business rule engines or decision logic
- Streaming infrastructure or event sourcing
- Persistence or caching of match results

These concerns belong to the consumer's application or infrastructure layers.

## Consequences
- The library remains small, focused, and easy to integrate across different IoT and event-processing systems.
- Consumers retain full control over storage, transport, and application logic.
- Development can concentrate on correctness and performance of temporal operations.
- Additional layers or integrations will not be added to the project, keeping the library boundary clear.
- The library has zero external dependencies, making it safe to use in any .NET environment.

## Related
- [ADR-4 — No Application and Infrastructure Layers](4-no-application-and-infrastructure.md)
- [ADR-7 — No External Dependencies](7-no-external-dependencies.md)
- [ADR-16 — Streaming and Late Arrivals Out of Scope](16-streaming-and-late-arrivals-out-of-scope.md)

# ADR-12 — Multiple Candidate Sets Matching

## Status
Accepted

## Context
In real-world IoT, IIoT and event-processing systems, a single anchor event is often correlated against multiple heterogeneous candidate sources, such as:

- signals from different machines,
- measurements from independent sensors,
- events coming from separate communication channels.

An initial idea was to support matching a single anchor collection against multiple candidate collections in one matcher invocation.

However, this introduces significant challenges:

- C# generics do not allow a single strongly-typed visitor to safely handle multiple unrelated candidate types.
- A single matcher invocation with heterogeneous candidate sets leads to:
    - complex and unclear visitor contracts,
    - combinatorial explosion of generic parameters,
    - unclear separation of responsibilities.

At the same time, the core matching logic is inherently defined as a relationship between one anchor type, and one candidate type, under a single, well-defined matching configuration.

## Decision
The library explicitly distinguishes between core matching and orchestration of multiple matches.

### Core matching
The core matching engine operates on:
 - a single anchor collection,
 - a single candidate collection,
 - a single matching configuration,
 - a single result buffer.

This core API ensures:
- minimal complexity,
- maximal performance,
- strong static typing.

### Orchestration
Correlating a single anchor collection against multiple candidate collections is defined as an orchestration concern, not a responsibility of the core matcher.

Orchestration is performed by the caller, for example:
```csharp
MatchTemporal.Points.With.Points(anchors, candidatesA, policyA, ref bufferA);
MatchTemporal.Points.With.Points(anchors, candidatesB, policyB, ref bufferB);
MatchTemporal.Points.With.Points(anchors, candidatesC, policyC, ref bufferC);
```

Optional orchestration helpers may be provided, but they:
- compose multiple core matcher invocations,
- do not introduce new matching semantics.

Global concerns such as:
- determining whether an anchor matched any candidate across all sources,
- are handled at the orchestration layer.

## Consequences
- Core matching remains simple, deterministic, and strongly typed.
- Each matcher invocation operates on exactly one anchor–candidate type pair.
- Orchestration logic is explicit and visible in user code.
- The design scales naturally to any number of candidate sources without API explosion.
- The architecture cleanly follows the principle: **"Matching is computation; orchestration is coordination."**

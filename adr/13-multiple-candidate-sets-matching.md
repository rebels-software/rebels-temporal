# ADR-15 — Multiple candidate sets matching

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
    - increased complexity in source generators,
    - unclear separation of responsibilities.

At the same time, the core matching logic is inherently defined as a relationship between one anchor type, and one candidate type, under a single, well-defined matching configuration.

## Decision
The library explicitly distinguishes between core matching and orchestration of multiple matches.

### Core matching
The core matching engine operates on:
 - a single anchor collection,
 - a single candidate collection,
 - a single matching configuration,
 - a single strongly-typed visitor.

 ``` csharp
 Match(
    IReadOnlyList<TAnchor> anchors,
    IReadOnlyList<TCandidate> candidates,
    MatchOptions options,
    IPairMatchVisitor<TAnchor, TCandidate> visitor);
 ```

This core API is the only target of Roslyn source generators, ensuring:
- minimal complexity,
- maximal performance,
- predictable code generation,
- strong static typing.

### Orchestration
Correlating a single anchor collection against multiple candidate collections is defined as an orchestration concern, not a responsibility of the core matcher.

Orchestration is performed by the caller, for example:
 ``` csharp
matcher.Match(anchors, candidatesA, optionsA, visitorA);
matcher.Match(anchors, candidatesB, optionsB, visitorB);
matcher.Match(anchors, candidatesC, optionsC, visitorC);
 ```

Optional orchestration helpers may be provided, but they:
- compose multiple core matcher invocations,
- do not introduce new matching semantics,
- do not participate in source generation.

Global concerns such as:
- determining whether an anchor matched any candidate across all sources,
- invoking OnMiss(anchor) only once,
- are handled at the orchestration layer.

## Consequences
- Core matching remains simple, deterministic, and strongly typed.
- Each matcher invocation operates on exactly one anchor–candidate type pair.
- The visitor contract stays minimal and type-safe.
- Source generators remain focused on a single, well-defined responsibility.
- Orchestration logic is explicit and visible in user code.
- The design scales naturally to any number of candidate sources without API explosion.
- The architecture cleanly follows the principle: **“Matching is computation; orchestration is coordination.”**
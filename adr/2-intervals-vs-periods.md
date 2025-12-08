# ADR-2 – Why Intervals Instead of Periods in the Public API

## Status
Accepted

## Context
Domain models across industries use concepts like “charging period”, “presence period”, or “operation period”.  
These concepts carry semantic meaning that depends on the domain and business context.  
However, the library requires a neutral, mathematical structure for performing overlap detection, containment checks, adjacency analysis, and other interval-based operations.

Using a domain-heavy term like "Period" in the API introduces semantic assumptions that do not belong in a temporal reasoning engine.

## Decision
Expose a neutral abstraction named `ITemporalInterval` to represent any time span with a start and end, and encourage consumers to map their domain-specific “period” concepts onto this interface.

## Consequences
- The library aligns with formal mathematical models such as Allen’s interval algebra.
- The API remains universal and domain-agnostic.
- Domain models maintain full semantic richness without affecting the temporal engine.
- Temporal operations (overlap, containment, intersection, etc.) remain predictable and clearly defined.

# ADR-0006 – Exact Matching and Window Matching as Core Matchers

## Status
Accepted

## Context
Temporal data from IoT and event-driven systems commonly requires:
1) correlating events that occurred at nearly the same instant,
2) matching events within a time window around a reference point.

These two operations appear consistently across domains (telemetry, access control, logistics, sensor fusion).
Other matching patterns can be built on top of these two primitives.

## Decision
Define two fundamental matching operations in Rebels.Temporal:
- Exact Matching (with optional tolerance)
- Window-Based Matching (backward and forward offsets around an anchor event)

All other correlation mechanisms will be conceptually built on these two foundations.

## Consequences
- The library remains simple and focused.
- Users gain powerful primitives for building domain-specific correlations.
- Additional matchers (e.g., cascading windows, multi-source alignment) can extend these primitives without disturbing the core API.

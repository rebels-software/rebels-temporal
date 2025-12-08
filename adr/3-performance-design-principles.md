# ADR-3 – Design Principles for the Public API (Performance-First)

## Status
Accepted

## Context
Rebels.Temporal is intended for processing large volumes of time-based data from IoT devices, telemetry systems, and event-driven architectures.  
These scenarios require predictable performance, minimal memory allocations, and efficient hot-path algorithms that operate at scale.

Certain .NET features (e.g., LINQ, dynamic allocations, boxing, delegate capture) can introduce unnecessary overhead and GC pressure.

## Decision
Adopt a performance-first design strategy for all public API and internal core algorithms:

- Prefer `ReadOnlySpan<T>` and `Span<T>` for data access.
- Avoid LINQ in all performance-critical paths.
- Avoid allocating intermediate collections unless explicitly requested by the user.
- Prefer `struct` enumerators and minimal abstractions.
- Keep algorithms deterministic in complexity (documenting O(n log n), O(n), etc.)
- Do not introduce external dependencies.

## Consequences
- The library behaves predictably under high load and scales to millions of events.
- Consumers can rely on the library in latency-sensitive systems.
- Contributors must follow performance constraints when adding new features.
- Some APIs may appear more low-level, but careful documentation offsets this complexity.

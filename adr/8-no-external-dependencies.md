# ADR-0008 – No External Dependencies Beyond .NET BCL

## Status
Accepted

## Context
External dependencies introduce risks: version conflicts, performance overhead, licensing issues, and unpredictable behaviors.
Temporal processing often occurs in performance-sensitive pipelines, sometimes in restricted environments (edge devices, embedded .NET runtimes).
Using only the .NET Base Class Library ensures reliability, compatibility, and minimal overhead.

## Decision
Avoid external NuGet dependencies and rely exclusively on .NET BCL (System.*, Microsoft.* packages that ship with the runtime).

## Consequences
- No dependency conflicts or supply-chain concerns.
- Predictable deployment and performance characteristics.
- Contributors work within a controlled and stable API surface.
- All algorithms are custom-tailored, optimized, and deterministic.

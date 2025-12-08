# ADR-0005 – No Application or Infrastructure Layers in This Repository

## Status
Accepted

## Context
Layered architectures (Application, Domain, Infrastructure) are typical in full applications or services.
Rebels.Temporal, however, is a pure domain library focused solely on temporal reasoning.
It does not require persistence, messaging, networking, orchestration, or any form of I/O.

Introducing Application or Infrastructure layers would suggest responsibilities that the library does not and should not have.

## Decision
Do not include Application or Infrastructure layers in the project.
The library will expose only domain concepts (models, interfaces, relations) and core algorithms (matchers, analyzers).

## Consequences
- The project remains minimal, predictable, and domain-focused.
- Contributors know exactly what belongs inside the repository.
- There is no structural ambiguity (no empty or stub layers waiting to be misused).
- The library is easy to understand for AI assistants and new contributors.

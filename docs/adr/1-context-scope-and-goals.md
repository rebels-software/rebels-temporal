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

## Consequences
- The library remains small, focused, and easy to integrate across different IoT and event-processing systems.
- Consumers retain full control over storage, transport, and application logic.
- Development can concentrate on correctness and performance of temporal operations.
- Additional layers or integrations will not be added to the project, keeping the library boundary clear.

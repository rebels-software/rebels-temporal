# ADR-14 — DateTime vs DateTimeOffset Support

## Status
Accepted

## Context
Consumers of the library may rely on either `DateTime` or `DateTimeOffset` as their event timestamp types.  
However, internal temporal comparison must be deterministic and consistent.  
`DateTime` with `Kind.Local` or `Kind.Unspecified` easily leads to incorrect comparison results.  
`DateTimeOffset` provides richer semantics but complicates internal logic.

## Decision
The library will support both timestamp types as input:

1. Core implementation uses `DateTime` in UTC.
2. Overloads accepting `DateTimeOffset` will convert timestamps using `.UtcDateTime` before delegating to the core implementation.

## Consequences

 - Library remains flexible for consumers using either timestamp representation.
 - Internal logic stays consistent by normalizing all values to UTC.
 - Avoids duplicated match logic for each timestamp type.
 - Prevents common timezone and DST-related errors.
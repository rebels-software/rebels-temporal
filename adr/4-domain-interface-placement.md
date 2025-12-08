# ADR-0004 – Placement of Domain Interfaces in `/Domain`

## Status
Accepted

## Context
The library defines several fundamental abstractions such as `ITemporalEvent` and `ITemporalInterval`.  
These are not infrastructure concerns or technical abstractions; they represent the core domain vocabulary of the temporal bounded context.  
In traditional DDD-style architectures, domain concepts belong in the Domain layer rather than in application or infrastructure layers.

## Decision
Place all semantic abstractions—including `ITemporalEvent`, `ITemporalInterval`, and temporal relation definitions—within the `/Domain` directory structure.  
Avoid creating an `/Abstractions` or `/Infrastructure` directory for these types.

## Consequences
- The domain model remains cohesive and easy to navigate.
- Contributors clearly understand which concepts form the ubiquitous language of the library.
- The API communicates intent more precisely by grouping semantically related types.
- Future additions to the domain vocabulary will naturally fit into the same structure.

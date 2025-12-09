# ADR-0013 — Visitor API Design

## Status
Accepted

## Context
For maximum performance in IoT or streaming scenarios, some consumers need results delivered immediately, without allocating collections or constructing result containers.  
`IEnumerable<T>` introduces unavoidable allocations, even when used minimally.

A zero-allocation, callback-based API is necessary for advanced usage.

## Decision
Two visitor interfaces will be introduced:

```csharp
public interface IPairMatchVisitor<TA, TB>
{
    void OnMatch(TA anchor, TB b);
}

public interface IGroupMatchVisitor<TA, TB>
{
    void OnGroup(TA anchor, ReadOnlySpan<TB> matches);
}
```

TimestampMatcher will provide two dedicated visitor methods:
```csharp
VisitPairs(..., IPairMatchVisitor);
VisitGroups(..., IGroupMatchVisitor);
```

## Consequences
 - Enables real-time match processing without any memory allocations.
 - Clear semantic separation between pair and group processing.
 - Ideal for pipelines, streaming processors, and high-throughput event systems.
 - Simplifies unit testing and reasoning about behavior.
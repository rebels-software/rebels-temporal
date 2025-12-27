# ADR-14 — DateTime vs DateTimeOffset Support

## Status

Accepted

## Context

Consumers of the library may currently use either DateTime or DateTimeOffset to represent timestamps.
However, deterministic temporal comparison requires a single, unambiguous time representation.

Supporting both timestamp types introduces complexity:
- DateTime may have Kind.Local, Kind.Utc, or Kind.Unspecified, which can lead to silent and incorrect comparison results.
- Converting DateTime to DateTimeOffset requires selecting an offset, which cannot be inferred reliably.
- Handling two APIs increases cognitive load, encourages misuse, and complicates Source Generator logic.
- To ensure correctness and maintain clean extensibility, the library must expose a single timestamp type.

## Decision

The library will use DateTimeOffset as the only timestamp type across all public APIs, including:
- IMatchableByPoint.At
- IMatchableByInterval.Start
- IMatchableByInterval.End
- all future timestamp-based interfaces
- match strategies and relations
- any generated code that operates on time values

The library will not perform implicit DateTime → DateTimeOffset conversion.

Consumers working with DateTime must convert explicitly before invoking library methods.

This ensures:
- precision,
- timezone awareness,
- deterministic behavior,
- predictability in both runtime logic and generated code.

## Consequences

### Positive

- A single, unambiguous model for timestamps across entire library.
- Eliminates ambiguity caused by DateTimeKind and DST transitions.
- Greatly simplifies implementation and improves safety.
- Reduces branching and complexity in the Source Generator.
- Matches real-world IoT and distributed system best practices.
- Prevents silent bugs caused by implicit conversions.

### Negative
- Consumers using DateTime must perform explicit conversion:

```csharp
var dto = new DateTimeOffset(dt);
```

- Slightly more verbose for users who previously relied on local DateTime values.

## Rejected Alternatives
- Supporting both DateTime and DateTimeOffset

   Rejected because duality adds ambiguity and complexity, increases surface area, and encourages misuse.

- Normalizing all DateTime to UTC internally

   Rejected because the library would still have to guess offsets for Kind.Unspecified.

- Offering dual interfaces (DateTime and DateTimeOffset)

   Rejected due to API expansion, generator duplication, and no meaningful user benefit.
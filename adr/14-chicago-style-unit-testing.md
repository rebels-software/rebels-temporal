# ADR-16 — Chicago-Style Unit Testing Strategy

## Status
Accepted

## Context
Unit testing strategies generally fall into two schools of thought:

1. **London School (mockist)** — focuses on isolation through extensive mocking of dependencies, testing interactions between objects.
2. **Chicago School (classicist)** — focuses on testing behavior and final state using real implementations, minimizing mocks.

Rebels.Temporal is a domain-focused library with:
- pure algorithmic logic,
- deterministic temporal operations,
- minimal external dependencies (only .NET BCL),
- immutable value types and structs,
- high-performance, side-effect-free computations.

In such environments, mocking dependencies provides little value and introduces fragility:
- Tests become coupled to implementation details rather than observable behavior.
- Refactoring internal structure breaks tests even when external behavior remains unchanged.
- Mock setup obscures the actual logic being tested.
- Mocks cannot verify correctness of complex temporal algorithms.

## Decision
Adopt the **Chicago School (classicist) approach** to unit testing:

### Principles
1. **Test small units of behavior, not isolated classes**
   A "unit" represents a cohesive piece of logic together with its collaborators, not a single class in complete isolation.

2. **Use real implementations wherever possible**
   Dependencies should be real objects unless they involve I/O, randomness, or external systems (none of which exist in this library).

3. **Minimize use of mocks and test doubles**
   Mocks are only acceptable when testing integration points that do not exist in the core domain (e.g., custom visitor implementations in tests).

4. **Assert on final state and observable outcomes**
   Tests verify the result of operations (matched pairs, temporal relations, computed intervals) rather than method call sequences.

5. **Favor refactoring-resistant tests**
   Tests should survive internal restructuring as long as the public API contract and behavior remain unchanged.

### Practical Guidelines
- Test domain types (`MatchPair`, `TemporalRelation`, etc.) using concrete instances.
- Test matchers (`TimestampMatcher`) with real collections of temporal events.
- Verify results by inspecting returned values, not by asserting that internal methods were called.
- When testing visitor patterns, implement simple test-specific visitors that collect results for assertion.
- Use test data builders or factory methods to create readable, intention-revealing test fixtures.
- Organize tests by behavior and scenario, not by class structure.

### Example: Testing MatchPair
```csharp
[Fact]
public void MatchPair_Should_Store_Anchor_And_Candidate()
{
    // Arrange
    var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero));
    var candidate = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 5, TimeSpan.Zero));

    // Act
    var pair = new MatchPair<TestEvent, TestEvent>(
        anchor,
        candidate,
        MatchType.PointExact);

    // Assert
    Assert.Same(anchor, pair.Anchor);
    Assert.Same(candidate, pair.Candidate);
    Assert.Equal(MatchType.PointExact, pair.MatchType);
    Assert.Null(pair.Relation);
}
```

### Example: Testing TimestampMatcher (when implemented)
```csharp
[Fact]
public void Match_Should_Find_Exact_Timestamp_Pairs()
{
    // Arrange
    var anchors = new[]
    {
        new SensorReading(DateTimeOffset.Parse("2025-01-01T12:00:00Z"), 23.5),
        new SensorReading(DateTimeOffset.Parse("2025-01-01T12:01:00Z"), 24.1)
    };

    var candidates = new[]
    {
        new LogEntry(DateTimeOffset.Parse("2025-01-01T12:00:00Z"), "START"),
        new LogEntry(DateTimeOffset.Parse("2025-01-01T12:02:00Z"), "END")
    };

    var visitor = new CollectingVisitor<SensorReading, LogEntry>();

    // Act
    TimestampMatcher.Match(anchors, candidates, visitor);

    // Assert
    Assert.Single(visitor.Matches);
    Assert.Equal(23.5, visitor.Matches[0].Anchor.Value);
    Assert.Equal("START", visitor.Matches[0].Candidate.Message);
}
```

## Consequences

### Positive
- Tests remain valid during internal refactoring (moving methods, renaming private members, changing algorithms).
- Test code closely resembles real usage, serving as executable documentation.
- Fewer dependencies on mocking frameworks (aligns with ADR-8: no external dependencies).
- Tests validate actual correctness of temporal algorithms, not just interaction contracts.
- Higher confidence that the library works correctly in production scenarios.
- Simpler test code that is easier to read and maintain.

### Negative
- Some tests may require more setup compared to heavily mocked alternatives.
- Testing complex scenarios might involve constructing larger object graphs.
- Contributors unfamiliar with Chicago School may initially expect more mocking.

### Mitigations
- Provide reusable test fixtures and builder utilities to simplify test setup.
- Document testing patterns in contributor guidelines.
- Use factory methods and helper classes to reduce boilerplate.

## Rejected Alternatives

### London School (extensive mocking)
Rejected because:
- The library has no I/O, state mutation, or unpredictable side effects that would benefit from isolation.
- Mocking pure domain logic obscures actual behavior and creates brittle tests.
- Temporal correctness cannot be verified through interaction-based assertions.

### Hybrid approach with selective mocking
Rejected because:
- Introduces inconsistency in testing philosophy.
- The domain is small and cohesive enough to test with real implementations throughout.
- No external dependencies exist that would require mocking (ADR-8).

## References
- Martin Fowler: [Mocks Aren't Stubs](https://martinfowler.com/articles/mocksArentStubs.html)
- Kent Beck: *Test-Driven Development by Example* (Chicago School origins)
- Steve Freeman & Nat Pryce: *Growing Object-Oriented Software, Guided by Tests* (London School)

# INV-7 — Single Anchor-Candidate Pair per Match Operation

## Rule

Each core matching operation MUST operate on exactly one anchor type and one candidate type. Heterogeneous type mixing within a single match invocation is NOT permitted.

## Formal Definition

For any matching operation `Match`:

```
Match : (Anchors<TA>, Candidates<TC>, Policy, Buffer<TA,TC>) → Results<TA,TC>
```

Where `TA` and `TC` are single, statically-known types.

## Meaning

A matching operation correlates elements from one anchor collection with elements from one candidate collection. Both collections are homogeneous (single type each). Correlating against multiple candidate sources requires multiple separate match invocations; orchestration of multiple sources is the caller's responsibility.

## Implications

- Each match invocation processes exactly one anchor type and one candidate type.
- Algorithms MAY assume homogeneous, statically-typed input collections.
- Algorithms are NOT required to handle mixed types, dynamic dispatch, or runtime type checks.
- Multi-source correlation MUST be implemented by invoking multiple match operations.
- Results are strongly typed to the anchor-candidate pair.

## Forbidden

- Accepting multiple candidate collections of different types in a single match call.
- Using heterogeneous collections (e.g., `object[]` or interfaces hiding multiple types).
- Performing dynamic type dispatch or runtime type checks within matching logic.
- Building "universal" matchers that accept arbitrary type combinations.

## Notes

- Callers correlating against multiple candidate sources invoke the matcher multiple times.
- Cross-source correlation logic (e.g., "anchor matched in any source") is orchestration, not matching.

## Related

- [ADR-12 — Multiple Candidate Sets Matching](../adr/12-multiple-candidate-sets-matching.md)

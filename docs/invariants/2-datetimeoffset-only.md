# INV-2 — DateTimeOffset Only

## Rule

All temporal values MUST be represented as `DateTimeOffset`. The use of `DateTime` is NOT permitted.

## Formal Definition

For any property, parameter, or return value representing a moment in time:

```
type(timestamp) = DateTimeOffset
```

## Meaning

A temporal value represents an unambiguous moment in time. `DateTimeOffset` includes both the instant and the UTC offset, ensuring that comparisons between timestamps from different sources are deterministic and correct. `DateTime` lacks this guarantee due to its ambiguous `Kind` property.

## Implications

- All temporal interfaces and APIs MUST use `DateTimeOffset`.
- Algorithms MAY assume that all timestamps are `DateTimeOffset` and can be compared directly.
- Algorithms are NOT required to handle `DateTime` values or perform conversions.
- Timestamp comparisons are always well-defined and timezone-aware.

## Forbidden

- Using `DateTime` in any public API surface.
- Performing implicit `DateTime` → `DateTimeOffset` conversions within the library.
- Providing dual APIs (one for `DateTime`, one for `DateTimeOffset`).
- Storing or processing temporal values as `DateTime` internally.
- Assuming or inferring UTC offset for unspecified `DateTime` values.

## Notes

- Callers using `DateTime` MUST convert to `DateTimeOffset` before invoking library APIs.
- This invariant ensures deterministic behavior across distributed systems and time zones.

## Related

- [ADR-11 — DateTime vs DateTimeOffset Support](../adr/11-datetime-vs-datetimeoffset-support.md)

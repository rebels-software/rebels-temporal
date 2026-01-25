# INV-4 — Single Namespace

## Rule

All public types MUST be declared in the `Rebels.Temporal` namespace. Sub-namespaces for public types are NOT permitted.

## Formal Definition

For every public type `T`:

```
namespace(T) = "Rebels.Temporal"
```

## Meaning

The library exposes its entire public API through a single, flat namespace. Consumers access all functionality with one import statement. Internal folder organization does not affect the public namespace structure.

## Implications

- All public interfaces, classes, structs, and enums MUST use `Rebels.Temporal` as their namespace.
- Consumers MAY assume that a single `using Rebels.Temporal;` provides access to the complete API.
- New public types MUST be added to the same namespace without requiring additional imports.
- Internal/private types MAY use any namespace organization.

## Forbidden

- Declaring public types in sub-namespaces (e.g., `Rebels.Temporal.Matching`, `Rebels.Temporal.Domain`).
- Requiring consumers to add multiple `using` statements for different parts of the API.
- Coupling namespace structure to internal folder organization.

## Notes

- Internal folder structure is for code organization only and does not dictate namespaces.
- This invariant applies only to public types; internal types have no namespace restrictions.

## Related

- [ADR-9 — Namespace Strategy](../adr/9-namespace-strategy.md)

# INV-5 — No External Dependencies

## Rule

The library MUST NOT depend on any NuGet packages beyond the .NET Base Class Library (BCL). Third-party runtime dependencies are NOT permitted.

## Formal Definition

```
runtime_dependencies ⊆ { System.*, Microsoft.* shipped with .NET runtime }
third_party_runtime_dependencies = ∅
```

## Meaning

The library is fully self-contained, relying only on APIs provided by the .NET runtime. This eliminates supply-chain risks, version conflicts, and deployment complexity. All algorithms and utilities are implemented within the library.

## Implications

- The library MAY use any `System.*` or `Microsoft.*` namespace shipped with the .NET runtime.
- The library MUST NOT reference third-party NuGet packages as runtime dependencies.
- Development-time dependencies (test frameworks, analyzers) MAY be used but MUST NOT be included in the published package.
- All temporal algorithms and utilities MUST be implemented internally.

## Forbidden

- Adding third-party NuGet packages as runtime dependencies.
- Relying on external libraries for temporal logic, serialization, or utilities.
- Including development dependencies in the published package.

## Notes

- Development dependencies (e.g., test frameworks, benchmarking tools) are permitted with `PrivateAssets="all"`.
- The published NuGet package MUST have zero transitive runtime dependencies.

## Related

- [ADR-7 — No External Dependencies Beyond .NET BCL](../adr/7-no-external-dependencies.md)

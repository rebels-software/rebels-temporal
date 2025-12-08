# ADR-0009 – Versioning Strategy (Semantic Versioning for .NET Libraries)

## Status
Accepted

## Context
Microsoft recommends that .NET libraries follow Semantic Versioning (SemVer) to communicate compatibility, breaking changes, and evolution of the public API.  
The official guidance emphasizes using MAJOR.MINOR.PATCH to indicate the nature of changes, rather than coupling library versions to .NET runtime versions.  
Reference: https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning

Rebels.Temporal is intended to be stable, predictable, and easy for consumers to adopt.  
Following true SemVer makes compatibility expectations clear and avoids confusion when future .NET versions are released.

## Decision
Adopt standard Semantic Versioning (SemVer 2.0) for all releases of the library:

- **MAJOR** — incremented when a breaking change to the public API is introduced.
- **MINOR** — incremented when new features are added without breaking existing behavior.
- **PATCH** — incremented for bug fixes, performance improvements, and other non-breaking changes.

The version number will **not** be tied to the .NET runtime version (e.g., .NET 8 does not imply Rebels.Temporal 8.x.x).

## Consequences
- Consumers clearly understand whether an upgrade is safe based on SemVer semantics.
- The library remains compatible across multiple .NET runtime versions unless API-breaking changes require otherwise.
- Breaking changes are intentionally grouped into major releases.
- Versioning aligns with Microsoft’s official recommendations for .NET libraries.
- Future .NET runtime releases do not force artificial version bumps in the library.

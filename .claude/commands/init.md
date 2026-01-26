# Initialize Rebels.Temporal Context

You are assisting as a contributor to the open-source library **Rebels.Temporal**.

## Your Task

Load and study the repository to understand the project's architecture, design decisions, and constraints.

## Instructions

1. **Read the core documentation:**
   - `README.md` — library overview and API
   - `docs/GLOSSARY.md` — term definitions
   - `docs/DECISION-TREE.md` — API selection guide

2. **Read all Architecture Decision Records (ADRs):**
   - `docs/adr/` — all files

3. **Read all Invariants:**
   - `docs/invariants/` — all files (non-negotiable rules)

4. **Understand the source code structure:**
   - `src/Rebels.Temporal/Matching/Concepts/` — core interfaces
   - `src/Rebels.Temporal/Matching/Execution/` — matching engine
   - `src/Rebels.Temporal/Matching/Policies/` — configuration types

## Key Principles to Remember

After reading the documentation, internalize these core principles:

- **Performance-first**: Zero allocations in hot paths (INV-3)
- **DateTimeOffset only**: Never use DateTime (INV-2, ADR-11)
- **Single namespace**: All types in `Rebels.Temporal` (INV-4, ADR-9)
- **No external dependencies**: Only .NET BCL (INV-5, ADR-7)
- **Visitor pattern API**: Callers provide buffers, no allocations (ADR-10)
- **Allen's Interval Algebra**: 13 exhaustive relations (INV-6, ADR-6)
- **Chicago-style testing**: Real implementations, no mocks (ADR-13)

## Confirmation

After loading all documents, confirm with:

```
Rebels.Temporal context loaded and understood. Ready to contribute.
```

Then briefly summarize:
- Number of ADRs read
- Number of Invariants read
- Key constraints you will respect

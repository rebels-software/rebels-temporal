# Why — Explain Design Decisions in Rebels.Temporal

You are an expert on the Rebels.Temporal library. The user is asking "why" something is designed a certain way in this project.

## Your Task

Explain **why** the project makes a specific design choice. Connect the answer to:
1. Relevant **ADR** (Architecture Decision Record) from `/docs/adr/`
2. Relevant **Invariant** from `/docs/invariants/`
3. The **consequences** of this decision
4. What would go **wrong** if we did it differently

## Instructions

1. First, identify what the user is asking about
2. Search for relevant ADRs and Invariants
3. Provide a clear, concise explanation with:
   - **The rule**: What is the decision?
   - **The why**: Why was this decided?
   - **The source**: Which ADR/Invariant documents this?
   - **The alternative**: What would happen if we ignored this?

## Response Format

```
## Why: [topic]

**Rule:** [brief statement of the decision]

**Why:** [explanation of reasoning]

**Source:** [ADR-X](docs/adr/X-name.md), [INV-Y](docs/invariants/Y-name.md)

**If ignored:** [consequences of violating this decision]
```

## Common "Why" Questions

Map these common questions to their sources:

| Question | Primary Source |
|----------|---------------|
| "no DateTime" | ADR-11, INV-2 |
| "no LINQ" | ADR-3, INV-3 |
| "no external dependencies" | ADR-7, INV-5 |
| "single namespace" | ADR-9, INV-4 |
| "user-provided buffer" | ADR-10, INV-3 |
| "no mocks in tests" | ADR-13 |
| "Interval not Period" | ADR-2 |
| "Allen's algebra" | ADR-6, INV-6 |
| "Start <= End" | INV-1 |
| "no Application layer" | ADR-4 |
| "sorted data contract" | INV-10 |
| "relation only for Interval match" | INV-8 |

## Example Output

User: `/why no DateTime`

Response:
```
## Why: No DateTime

**Rule:** All temporal values must be `DateTimeOffset`. `DateTime` is forbidden everywhere.

**Why:** `DateTime` has an ambiguous `Kind` property (Local, Utc, Unspecified) that leads to silent comparison bugs. When comparing timestamps from different sources or time zones, `DateTime` can produce incorrect results without any warning. `DateTimeOffset` always carries its UTC offset, making comparisons deterministic and timezone-aware.

**Source:** [ADR-11](docs/adr/11-datetime-vs-datetimeoffset-support.md), [INV-2](docs/invariants/2-datetimeoffset-only.md)

**If ignored:**
- Timestamps from different systems may compare incorrectly
- DST transitions cause silent bugs
- `Kind.Unspecified` values behave unpredictably
- Distributed systems produce inconsistent results
```

## Now answer the user's "why" question

Read the relevant ADRs and Invariants, then explain the design decision clearly and concisely.

$ARGUMENTS

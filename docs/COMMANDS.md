# Rebels.Temporal — LLM Commands

This document describes the command system available when working with AI assistants (Claude, ChatGPT, Gemini, etc.).

---

## Available Commands

| Command | Description | When to Use |
|---------|-------------|-------------|
| `/init` | Initialize LLM context | At the start of a session, before beginning work |
| `/why`  | Explain design decisions | When you want to understand "why" something was designed a certain way |
| `/benchmark` | Run performance benchmarks | When you want to measure performance or compare implementations |

---

## /init

### Description

The `/init` command loads the full Rebels.Temporal project context into the LLM's memory. It should be used at the beginning of each new session with an AI assistant.

### Usage

Type `/init` in your conversation with the LLM.

### What It Does

After executing the command, the LLM will:

1. **Load the repository structure:**
   - `README.md` — library overview and API
   - `/docs` — full documentation
   - `/docs/adr` — Architecture Decision Records
   - `/docs/invariants` — non-negotiable system rules
   - `/src/Rebels.Temporal` — source code

2. **Understand the domain model:**
   - Temporal Events (point-in-time occurrences)
   - Temporal Periods vs Temporal Intervals
   - Time Windows
   - Temporal Relations (Allen's Interval Algebra)

3. **Learn the design principles:**
   - Performance-first design
   - Zero allocations in hot path
   - `DateTimeOffset` only
   - No external dependencies

4. **Confirm readiness:**
   ```
   Rebels.Temporal context loaded and understood. Ready to contribute.
   ```

### Initialization Prompt

```text
You are assisting as a contributor to the open-source library Rebels.Temporal.

Load and study the following repository structure, including its documentation and architecture decision records:
- README.md
- /docs (all files)
- /docs/adr (all Architecture Decision Records)
- /docs/invariants (all non-negotiable rules of the system)
- /src/Rebels.Temporal — the source code with domain model and matching engine

Your goals:
1. Understand the temporal domain model used by the library, including:
   - Temporal Events
   - Temporal Periods vs Temporal Intervals
   - Time Windows
   - Temporal Relations
2. Understand the design philosophy, performance principles, and boundaries of the project.
3. Respect all decisions declared in ADRs.
4. Provide answers and code suggestions consistent with the existing architecture.
5. When asked about new features, propose solutions aligned with the project's domain model and design constraints.

After loading all documents, acknowledge with:
"Rebels.Temporal context loaded and understood. Ready to contribute."
```

---

## /why

### Description

The `/why` command explains design decisions in the Rebels.Temporal codebase. It helps understand why something was implemented in a particular way.

### Usage

```
/why <question or context>
```

### Examples

```
/why why do we use DateTimeOffset instead of DateTime?
/why what is the reason for user-provided buffers?
/why why Allen's Interval Algebra?
/why explain the single namespace decision
```

### What It Does

The `/why` command:

1. Searches ADRs (Architecture Decision Records)
2. Checks system invariants
3. Analyzes code context
4. Returns an explanation with references to relevant documents

### Related Documents

- [ADRs](/docs/adr) — all architectural decisions
- [Invariants](/docs/invariants) — non-negotiable rules
- [GLOSSARY.md](/docs/GLOSSARY.md) — term definitions
- [DECISION-TREE.md](/docs/DECISION-TREE.md) — decision tree

---

## /benchmark

### Description

The `/benchmark` command runs performance benchmarks for the Rebels.Temporal library. It helps measure and compare the performance of different matching strategies and implementations.

### Usage

```
/benchmark [filter]
```

### Examples

```
/benchmark                    # Interactive mode - choose which benchmark to run
/benchmark sorted             # Run only sorted matching benchmarks
/benchmark unsorted           # Run only unsorted matching benchmarks
/benchmark consumer           # Run buffer implementation comparison
/benchmark all                # Run all benchmarks
```

### Available Benchmarks

| Benchmark | Description | Measures |
|-----------|-------------|----------|
| `PointMatchingSorted` | Point-to-point matching with sorted data | O(n+m) dual-pointer performance |
| `PointMatchingUnsorted` | Point-to-point matching with unsorted data | O(n×m) nested loop performance |
| `Consumer` | Buffer implementation approaches | ref struct vs generic struct with interface |

### What It Does

The `/benchmark` command:

1. Asks which benchmark to run (if no filter provided)
2. Builds the benchmark project in Release mode
3. Runs BenchmarkDotNet with the selected benchmarks
4. Reports results including mean time, allocations, and statistical analysis

### Running Directly

You can also run benchmarks directly from the command line:

```bash
cd benchmarks
dotnet run -c Release                              # Interactive mode
dotnet run -c Release -- --filter *Sorted*         # Filter by name
dotnet run -c Release -- --filter *PointMatching*  # Run all point matching
```

### Recent Benchmark Results

| Scenario | InputOrdering | Time | Complexity |
|----------|---------------|------|------------|
| 2k × 2k points | `Both` (sorted) | 56 μs | O(n+m) |
| 2k × 2k points | `None` (unsorted) | 14.4 ms | O(n×m) |

**Sorted data is ~255x faster.**

---

## Adding New Commands

To add a new command:

1. Add an entry to the "Available Commands" table above
2. Create a section with the command description containing:
   - Description
   - Usage
   - Examples
   - What It Does
3. Update README.md if the command is critical

### Command Section Format

```markdown
## /command-name

### Description
[Brief description of what the command does]

### Usage
[How to invoke the command]

### Examples
[Concrete usage examples]

### What It Does
[Detailed description of behavior]
```

---

## See Also

- [README.md](/README.md) — main documentation
- [GLOSSARY.md](GLOSSARY.md) — glossary of terms
- [DECISION-TREE.md](DECISION-TREE.md) — API selection guide

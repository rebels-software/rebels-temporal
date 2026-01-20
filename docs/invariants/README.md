# Invariants

## List of Invariants

| ID | Name | Summary |
|----|------|---------|
| [INV-1](1-interval-start-end-constraint.md) | Interval Start-End Constraint | All intervals must satisfy `Start <= End` |
| [INV-2](2-datetimeoffset-only.md) | DateTimeOffset Only | All temporal values must be `DateTimeOffset` |
| [INV-3](3-no-allocations-in-hot-path.md) | No Allocations in Hot Path | Core algorithms must not allocate heap memory |
| [INV-4](4-single-namespace.md) | Single Namespace | All public types in `Rebels.Temporal` namespace |
| [INV-5](5-no-external-dependencies.md) | No External Dependencies | No NuGet packages beyond .NET BCL |
| [INV-6](6-allen-relations-exhaustive.md) | Allen Relations Exhaustive | Two intervals relate by exactly one of 13 relations |
| [INV-7](7-single-anchor-candidate-pair.md) | Single Anchor-Candidate Pair | Each match operates on one anchor and one candidate type |
| [INV-8](8-matchpair-relation-consistency.md) | MatchPair Relation Consistency | Relation required iff MatchType is Interval |
| [INV-9](9-timetolerance-non-negative.md) | TimeTolerance Non-Negative | Tolerance values must be >= 0 |
| [INV-10](10-input-ordering-validation.md) | Input Ordering Validation | Declared ordering must be validated at runtime |

---

## What Are Invariants?

**Invariants** are non-negotiable, fundamental rules that must **always** hold true in the Rebels.Temporal library.

They define the **immutable constraints** that the system must satisfy at all times - regardless of implementation details, refactoring, or new features.

Think of invariants as the mathematical axioms or physical laws of the temporal domain:
- They cannot be violated without breaking the fundamental correctness of the system
- They are **enforcement rules**, not design preferences
- They apply to all code: production logic, tests, examples, and documentation


## Why Do We Need Invariants?

### 1. **Correctness Guarantees**
Invariants ensure that the library behaves predictably and correctly under all circumstances.

Example: *"All intervals must satisfy `Start <= End`"* prevents undefined behavior in Allen's Interval Algebra.

### 2. **Prevent Regression**
Once an invariant is established, it acts as a permanent safeguard.
Any change that violates an invariant should be reconsidered such as described below.

### 3. **Simplify Reasoning**
Contributors can rely on invariants as facts when writing or reviewing code.

Example: Knowing that *"all temporal values are `DateTimeOffset`"* eliminates entire classes of timezone bugs.

### 4. **API Stability**
Invariants protect the public API from accidental breaking changes.

Example: *"All public types must be in the `Rebels.Temporal` namespace"* ensures consumers never need to add new `using` statements.

### 5. **Performance Contracts**
Invariants define non-negotiable performance characteristics.

Example: *"Matching algorithms must not allocate memory in the hot path"* guarantees low GC pressure in IoT pipelines.

---

## What Happens When an Invariant is Violated?

### Severity: **CRITICAL**

A violation of an invariant is a serious issue that requires immediate attention.
However, the response depends on the nature of the violation.

---

### Scenario 1: Code Violates an Invariant

**This is the most common case** and indicates a bug or oversight in the implementation.

#### Immediate Actions:

1. **Stop the Work**
   Do not merge the pull request. Do not deploy the code.

2. **Assess the Violation**
   - Is this a bug in new code?
   - Is this a regression introduced by refactoring?
   - Does an existing part of the codebase violate the invariant?

3. **Fix the Code**
   Modify the implementation to satisfy the invariant.

4. **Add a Regression Test**
   Ensure the specific violation cannot happen again by adding automated verification.

5. **Update CI Pipeline (if needed)**
   If the violation was not caught automatically, add a check to prevent future occurrences.

---

### Scenario 2: Invariant Must Be Violated (Exceptional Cases)

**This should be extremely rare** but can happen when:
- Introducing a fundamentally new capability that conflicts with an existing invariant
- Supporting a valid use case that the invariant inadvertently blocks
- Implementing a critical performance optimization that requires bending a rule

#### Decision Process:

1. **Justify the Violation**
   Document **why** the invariant must be violated. What are the benefits? What are the risks?

2. **Evaluate Alternatives**
   Can the goal be achieved without violating the invariant?
   - Can the feature be designed differently?
   - Can the invariant be refined instead of violated?

3. **Gain Consensus**
   The decision to violate an invariant must be reviewed and approved by core maintainers.

4. **Document the Exception**
   Clearly explain:
   - Which invariant is being violated
   - Why the violation is necessary
   - What safeguards are in place to minimize risk
   - Whether this is a temporary or permanent violation

5. **Isolate the Violation**
   If possible, limit the scope of the violation to a well-defined, narrow context.

6. **Add Warnings**
   Use code comments and documentation to clearly mark the violation and explain the rationale.

**Example:**

An invariant states *"No allocations in hot path"*. However, supporting a new scenario (e.g., dynamic regex-based matching) may require allocations.

The team decides this is acceptable because:
- The feature is opt-in (doesn't affect existing users)
- The allocation is clearly documented
- Performance impact is measured and acceptable

---

### Scenario 3: Invariant is Incorrectly or Incompletely Defined

**This indicates a problem with the invariant itself**, not the code.

An invariant may be:
- **Too strict:** Blocking valid use cases unnecessarily
- **Ambiguous:** Open to multiple interpretations
- **Incomplete:** Failing to cover edge cases
- **Obsolete:** No longer relevant due to architectural changes

#### Resolution Process:

1. **Identify the Problem**
   Document what's wrong with the current invariant definition.
   - What valid scenario does it incorrectly reject?
   - What ambiguity is causing confusion?
   - What edge case is missing?

2. **Propose a Refinement**
   Write a precise, improved version of the invariant that:
   - Corrects the flaw
   - Maintains the original intent
   - Is clear and unambiguous

3. **Review and Approve**
   The refinement must be reviewed by core maintainers.

4. **Update the Invariant**
   Replace the old definition with the refined one.
   Add a "Revision History" section to the invariant file documenting the change.

5. **Validate Against Existing Code**
   Ensure the refined invariant:
   - Still holds for all existing code
   - Allows the previously blocked scenario

**Example:**

An invariant states *"All intervals must have `Start < End`"*.

However, this incorrectly rejects zero-duration intervals (`Start == End`), which are valid in temporal logic.

The invariant is refined to *"`Start <= End`"*.

---

### Summary of Response Paths

| Scenario | Action |
|----------|--------|
| **Code violates invariant** | Fix the code, add tests |
| **Invariant must be violated (rare)** | Justify, document, isolate, add warnings |
| **Invariant is poorly defined** | Refine the invariant, update documentation |

In all cases, **transparency and documentation** are critical.

---

## Can Invariants Ever Change?

**In principle: No.**

Invariants are designed to be permanent. Changing an invariant is equivalent to changing the fundamental nature of the library.

**In practice: Refinement is allowed, replacement is rare.**

### Refinement (Acceptable)
Clarifying an existing invariant without changing its fundamental intent.

Examples:
- Adding edge cases
- Improving wording
- Fixing ambiguities

### Replacement (Exceptional)
Completely changing or removing an invariant.

Example: Replacing *"Use `DateTime`"* with *"Use `DateTimeOffset`"*.

**Replacement requires:**
1. Core maintainer consensus
2. Clear documentation of the change and rationale
3. A migration guide (if the change affects public API)
4. Archival of the old invariant with a reference to why it was replaced

**Changing an invariant is a breaking change to the library's contract.**

It should be avoided whenever possible.

---

## Relationship to CI/CD

Invariants should be enforced automatically wherever possible:

- **Static Analysis:** Detect namespace violations, dependency introductions, etc.
- **Unit Tests:** Validate interval correctness, Allen relation exhaustiveness, etc.
- **Performance Tests:** Ensure zero allocations and complexity bounds
- **Contract Tests:** Verify API immutability and result consistency

The goal is to make it **impossible** to violate an invariant without a build failure.

---

## Summary

| Concept | Definition |
|---------|------------|
| **Invariant** | A non-negotiable rule that must always hold true |
| **Purpose** | Ensure correctness, prevent regression, maintain stability |
| **Violation** | Critical issue requiring fix, justification, or invariant refinement |
| **Changeability** | Refinement allowed; replacement requires consensus and documentation |
| **Enforcement** | Automated via tests, validation, and CI checks |

Invariants are the **bedrock** of Rebels.Temporal's reliability.

They define what it means for the library to be correct, performant, and stable.

**When in doubt, respect the invariants.**

**When an invariant seems wrong, challenge it - but with evidence and documentation.**

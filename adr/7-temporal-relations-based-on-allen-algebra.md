# ADR-7 – Temporal Relations Model Based on Allen’s Interval Algebra (Expanded)

## Status
Accepted

## Context
Temporal reasoning requires precise specification of how two time intervals relate.  
A well-known and mathematically formalized system for temporal relationships is *Allen’s Interval Algebra*,  
which defines exactly 13 mutually exclusive and collectively exhaustive relations between two intervals.

Adopting this algebra provides universally understood semantics and allows complex reasoning  
(overlaps, adjacency, ordering, containment, etc.) to be built on a formal foundation rather than ad-hoc logic.

## Decision
Rebels.Temporal will adopt the 13 relations defined in Allen’s Interval Algebra as the formal model for interval relations.  
These relations will be represented in the public API (likely via an enum or type),  
and all interval-based operations will use these definitions internally.

The 13 Allen relations are:

1. **Before (A < B)**  
   A ends before B starts.

2. **Meets (A m B)**  
   A ends exactly when B starts.

3. **Overlaps (A o B)**  
   A starts before B, but ends inside B.

4. **FinishedBy (A f B)**  
   A starts before B, and both end at the same time.

5. **Contains (A d B)**  
   A starts before B and ends after B.

6. **Starts (A s B)**  
   A starts at the same time as B, but ends earlier.

7. **Equals (A = B)**  
   A and B start and end at exactly the same times.

8. **StartedBy (A si B)**  
   A starts at the same time as B, but ends later.

9. **During (A di B)**  
   A starts after B starts and ends before B ends.

10. **Finishes (A fi B)**  
   A starts after B starts, and ends when B ends.

11. **OverlappedBy (A oi B)**  
   A starts inside B and ends after B.

12. **MetBy (A mi B)**  
   A starts exactly when B ends.

13. **After (A > B)**  
   A starts after B ends.

Each relation is a distinct, non-overlapping logical case,  
ensuring that any pair of intervals always falls into exactly one relation.

## Consequences
- Interval operations follow a mathematically sound, industry-recognized model.
- Contributors and users can rely on consistent, proven semantics when reasoning about time.
- Advanced features (clustering, segmentation, temporal graphs, temporal indexing)  
  can be built on this well-defined foundation.
- The API gains clarity because relations have unambiguous names and meanings.
- Algorithm correctness becomes easier to validate, document, and test.

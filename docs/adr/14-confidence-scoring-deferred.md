# ADR-14 — Confidence Scoring for Match Results

## Status
Deferred

## Context
During design review, confidence scoring was considered as a potential enhancement to match results. The idea is to provide a quality metric (0.0–1.0) for each `MatchPair`, indicating how "good" the match is within the allowed tolerance window.

For example, with a ±5 second tolerance:
- A match with 50ms time delta would score ~0.99 (near perfect)
- A match with 4.9s time delta would score ~0.02 (edge of tolerance)

### Considered Use Cases
- **Best match selection:** When an anchor has multiple candidates, select the one with highest confidence
- **Weak match filtering:** Discard matches below a quality threshold (e.g., < 0.5)
- **Analytics and reporting:** "85% of matches have confidence > 0.9"
- **Multi-tier analysis:** Run matching with multiple tolerances (±10s, ±5s, ±1s) and analyze distribution
- **ML pipelines:** Use confidence as a feature for downstream models

### Potential Implementation
```csharp
public readonly struct MatchPair<TAnchor, TCandidate>
{
    // ... existing properties ...

    /// <summary>
    /// Match quality score from 0.0 (edge of tolerance) to 1.0 (exact match).
    /// </summary>
    public double Confidence { get; }
}
```

Scoring formulas considered:
- **Point-to-Point:** `1.0 - (timeDelta / maxTolerance)`
- **Point-to-Interval:** Based on position within interval (centered = higher score)
- **Interval-to-Interval:** Jaccard index, overlap percentage, or relation-specific scoring

### Related Concept: Clock Skew Tolerance

In distributed IoT systems, device clocks are never perfectly synchronized. Even with NTP, typical drift is 10-200ms between devices. This raises a question: should clock skew be a separate configuration?

**Standalone clock skew tolerance** (without confidence scoring) adds complexity without clear value — users can simply include expected clock skew in their semantic tolerance.

**However**, clock skew becomes meaningful when combined with confidence scoring as a mechanism for defining **confidence zones**:

```
|<------------ semantic tolerance (±5s) ------------>|
|         |<-- clock skew (±200ms) -->|              |
|         |      HIGH CONFIDENCE      |              |
|  LOWER         (0.9 - 1.0)         LOWER          |
| CONFIDENCE                        CONFIDENCE       |
| (0.3 - 0.9)                       (0.3 - 0.9)     |
```

**Interpretation:**
- Match within clock skew → "same moment, difference is just clock drift" → **high confidence**
- Match outside clock skew but within semantic tolerance → "temporally related, not identical" → **lower confidence**

**Potential combined API:**
```csharp
var policy = new MatchPolicy
{
    AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(5)),  // outer boundary
    ClockSkewTolerance = TimeSpan.FromMilliseconds(200),                 // inner high-confidence zone
    ComputeConfidence = true
};

// Match at 50ms delta  → confidence ~0.95 (within clock skew)
// Match at 3s delta    → confidence ~0.40 (outside clock skew, within semantic)
// Match at 6s delta    → no match
```

**Conclusion:** Clock skew tolerance and confidence scoring are interdependent concepts. If confidence scoring is ever implemented, clock skew should be considered as the mechanism for defining confidence tiers rather than as a standalone feature.

## Decision
Deferred until real-world use cases emerge from library consumers.

### Rationale
1. **YAGNI (You Aren't Gonna Need It):** No concrete consumer requirement exists yet.
2. **Workarounds exist:** Users can achieve similar results by:
   - Adjusting tolerance to desired precision
   - Running multiple match passes with different tolerances
   - Post-processing `MatchPair` results with custom scoring logic
3. **Performance consideration:** Computing confidence adds overhead to every match, even when not needed.
4. **API design uncertainty:** Unclear whether scoring should be always-on, opt-in via policy, or a separate API.

## Consequences
- The library remains simpler and focused on core matching.
- Users needing confidence scoring must implement it externally for now.
- This decision is documented and can be revisited when real demand emerges.

## Revisit When
- A consumer requests this feature with a concrete, well-defined use case
- A pattern emerges of multiple users implementing custom scoring post-match
- Performance benchmarks show that optional scoring adds negligible overhead
- Clock skew handling becomes a requirement (consider implementing together)

## Related
- [ADR-10 — User-Provided Buffer Strategy](10-user-provided-buffer-strategy.md)
- [ADR-11 — DateTime vs DateTimeOffset Support](11-datetime-vs-datetimeoffset-support.md)
- [INV-3 — No Allocations in Hot Path](../invariants/3-no-allocations-in-hot-path.md)

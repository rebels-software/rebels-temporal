using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rebels.Temporal;

/// <summary>
/// Immutable, allocation-free representation of a temporal match
/// between an anchor event and a candidate event.
/// 
/// This structure may describe:
/// - point-to-point exact matches
/// - point-to-window matches
/// - window-to-point matches
/// - window-to-window matches
/// - interval-to-interval matches (Allen's algebra)
/// </summary>
public readonly struct MatchPair<TAnchor, TCandidate>
{
    /// <summary>
    /// The anchor event for which the match was found.
    /// </summary>
    public TAnchor Anchor { get; }

    /// <summary>
    /// The candidate event that matched the anchor.
    /// </summary>
    public TCandidate Candidate { get; }

    /// <summary>
    /// Indicates how the match was computed (exact, window-based, interval-based).
    /// </summary>
    public MatchType MatchType { get; }

    /// <summary>
    /// Optional temporal relation describing how the intervals relate
    /// (only applies when MatchType = IntervalRelation).
    /// </summary>
    public TemporalRelation? Relation { get; }

    public MatchPair(
        TAnchor anchor,
        TCandidate candidate,
        MatchType matchType,
        TemporalRelation? relation = null)
    {
        Anchor = anchor;
        Candidate = candidate;
        MatchType = matchType;
        Relation = relation;
    }
}
// Copyright (C) 2025 Rebels Software
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Rebels.Temporal;

/// <summary>
/// Provides high-performance temporal matching algorithms for correlating
/// anchors and candidates based on their temporal characteristics and
/// compile-time match policies.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TemporalMatcher{TPolicy}"/> exposes a set of specialized entry points
/// for matching temporal points and intervals using source-generated
/// implementations.
/// </para>
/// <para>
/// All matching semantics (tolerances, ordering guarantees, allowed relations)
/// are provided via <typeparamref name="TPolicy"/> and are therefore known
/// at compile time.
/// </para>
/// <para>
/// The runtime library itself contains no matching logic.
/// If a required generated implementation is missing, the build will fail.
/// </para>
/// </remarks>
/// <typeparam name="TPolicy">The compile-time match policy.</typeparam>
public static partial class TemporalMatcher<TPolicy>
    where TPolicy : IMatchPolicy
{
    #region Point to Point matching

    /// <summary>
    /// Matches temporal anchors and candidates that are both represented
    /// as points in time.
    /// </summary>
    /// <typeparam name="TAnchor">
    /// The anchor type. Must represent a single point in time.
    /// </typeparam>
    /// <typeparam name="TCandidate">
    /// The candidate type. Must represent a single point in time.
    /// </typeparam>
    /// <param name="anchors">
    /// The collection of anchor events to be matched.
    /// </param>
    /// <param name="candidates">
    /// The collection of candidate events to be matched against anchors.
    /// </param>
    /// <param name="visitor">
    /// A visitor that receives match and miss callbacks as matches are discovered.
    /// </param>
    public static void MatchPointToPoint<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        => MatchPointToPointGenerated(anchors, candidates, visitor);

    static partial void MatchPointToPointGenerated<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint;

    #endregion

    #region Point to Interval matching

    /// <summary>
    /// Matches temporal anchors represented as points in time
    /// against candidate intervals.
    /// </summary>
    public static void MatchPointToInterval<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        => MatchPointToIntervalGenerated(anchors, candidates, visitor);

    static partial void MatchPointToIntervalGenerated<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval;

    #endregion

    #region Interval to Point matching

    /// <summary>
    /// Matches temporal anchors represented as intervals
    /// against candidate points in time.
    /// </summary>
    public static void MatchIntervalToPoint<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        => MatchIntervalToPointGenerated(anchors, candidates, visitor);

    static partial void MatchIntervalToPointGenerated<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint;

    #endregion

    #region Interval to Interval matching

    /// <summary>
    /// Matches temporal anchors and candidates that are both represented
    /// as temporal intervals.
    /// </summary>
    public static void MatchIntervalToInterval<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        => MatchIntervalToIntervalGenerated(anchors, candidates, visitor);

    static partial void MatchIntervalToIntervalGenerated<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Determines the Allen's interval algebra relation between two temporal intervals.
    /// </summary>
    /// <param name="aStart">Start time of the first interval.</param>
    /// <param name="aEnd">End time of the first interval.</param>
    /// <param name="bStart">Start time of the second interval.</param>
    /// <param name="bEnd">End time of the second interval.</param>
    /// <returns>The temporal relation between the two intervals.</returns>
    private static TemporalRelation DetermineAllenRelation(
        System.DateTimeOffset aStart,
        System.DateTimeOffset aEnd,
        System.DateTimeOffset bStart,
        System.DateTimeOffset bEnd)
    {
        if (aEnd < bStart) return TemporalRelation.Before;
        if (aEnd == bStart) return TemporalRelation.Meets;
        if (aStart > bEnd) return TemporalRelation.After;
        if (aStart == bEnd) return TemporalRelation.MetBy;

        if (aStart == bStart && aEnd == bEnd) return TemporalRelation.Equal;
        if (aStart == bStart && aEnd < bEnd) return TemporalRelation.Starts;
        if (aStart == bStart && aEnd > bEnd) return TemporalRelation.StartedBy;
        if (aEnd == bEnd && aStart > bStart) return TemporalRelation.Finishes;
        if (aEnd == bEnd && aStart < bStart) return TemporalRelation.FinishedBy;

        if (aStart > bStart && aEnd < bEnd) return TemporalRelation.During;
        if (aStart < bStart && aEnd > bEnd) return TemporalRelation.Contains;

        if (aStart < bStart) return TemporalRelation.Overlaps;
        return TemporalRelation.OverlappedBy;
    }

    #endregion
}

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
/// This partial class contains group matching methods that deliver all
/// matching candidates for each anchor in a single callback via
/// <see cref="IGroupMatchVisitor{TAnchor,TCandidate}"/>.
/// </remarks>
public static partial class TemporalMatcher
{
    #region Point to Point group matching

    /// <summary>
    /// Matches temporal anchors and candidates that are both represented
    /// as points in time, delivering results grouped by anchor.
    /// </summary>
    /// <typeparam name="TAnchor">
    /// The anchor type. Must represent a single point in time.
    /// </typeparam>
    /// <typeparam name="TCandidate">
    /// The candidate type. Must represent a single point in time.
    /// </typeparam>
    /// <typeparam name="TPolicy">
    /// The compile-time match policy defining tolerances, ordering
    /// guarantees and allowed semantics.
    /// </typeparam>
    /// <param name="anchors">
    /// The collection of anchor events to be matched.
    /// </param>
    /// <param name="candidates">
    /// The collection of candidate events to be matched against anchors.
    /// </param>
    /// <param name="visitor">
    /// A visitor that receives grouped match results and miss callbacks.
    /// </param>
    public static void MatchPointToPointGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
        => MatchPointToPointGroupedGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchPointToPointGroupedGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    #endregion

    #region Point to Interval group matching

    /// <summary>
    /// Matches temporal anchors represented as points in time
    /// against candidate intervals, delivering results grouped by anchor.
    /// </summary>
    public static void MatchPointToIntervalGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
        => MatchPointToIntervalGroupedGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchPointToIntervalGroupedGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;

    #endregion

    #region Interval to Point group matching

    /// <summary>
    /// Matches temporal anchors represented as intervals
    /// against candidate points in time, delivering results grouped by anchor.
    /// </summary>
    public static void MatchIntervalToPointGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
        => MatchIntervalToPointGroupedGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchIntervalToPointGroupedGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    #endregion

    #region Interval to Interval group matching

    /// <summary>
    /// Matches temporal anchors and candidates that are both represented
    /// as temporal intervals, delivering results grouped by anchor.
    /// </summary>
    public static void MatchIntervalToIntervalGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
        => MatchIntervalToIntervalGroupedGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchIntervalToIntervalGroupedGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;

    #endregion
}

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
/// <see cref="TemporalMatcher"/> exposes a set of specialized entry points
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
public static partial class TemporalMatcher
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
    /// A visitor that receives match and miss callbacks as matches are discovered.
    /// </param>
    public static void MatchPointToPoint<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
        => MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchPointToPointGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    #endregion

    #region Point to Interval matching

    /// <summary>
    /// Matches temporal anchors represented as points in time
    /// against candidate intervals.
    /// </summary>
    public static void MatchPointToInterval<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
        => MatchPointToIntervalGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchPointToIntervalGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;

    #endregion

    #region Interval to Point matching

    /// <summary>
    /// Matches temporal anchors represented as intervals
    /// against candidate points in time.
    /// </summary>
    public static void MatchIntervalToPoint<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
        => MatchIntervalToPointGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchIntervalToPointGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy;

    #endregion

    #region Interval to Interval matching

    /// <summary>
    /// Matches temporal anchors and candidates that are both represented
    /// as temporal intervals.
    /// </summary>
    public static void MatchIntervalToInterval<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
        => MatchIntervalToIntervalGenerated<TAnchor, TCandidate, TPolicy>(
            anchors, candidates, visitor);

    static partial void MatchIntervalToIntervalGenerated<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy;

    #endregion
}

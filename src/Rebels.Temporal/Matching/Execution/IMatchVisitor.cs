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
/// Visitor interface for receiving temporal match results and unmatched anchor notifications.
/// </summary>
/// <typeparam name="TAnchor">Type of anchor events.</typeparam>
/// <typeparam name="TCandidate">Type of candidate events.</typeparam>
/// <remarks>
/// <para>
/// Implement this interface to receive match notifications during temporal matching.
/// For maximum performance, implement as a struct and use the generic overloads
/// in <see cref="MatchTemporal"/> which allow JIT devirtualization and inlining.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// public struct MyVisitor : IMatchVisitor&lt;Event, Event&gt;
/// {
///     public int MatchCount;
///     public int MissCount;
///
///     public void OnMatch(in Event anchor, in Event candidate,
///         int anchorIndex, int candidateIndex, MatchType type, TemporalRelation? relation)
///     {
///         MatchCount++;
///     }
///
///     public void OnUnmatchedAnchor(in Event anchor, int anchorIndex)
///     {
///         MissCount++;
///     }
/// }
/// </code>
/// </para>
/// </remarks>
public interface IMatchVisitor<TAnchor, TCandidate>
{
    /// <summary>
    /// Called when a match is found between an anchor and a candidate.
    /// </summary>
    /// <param name="anchor">The anchor event.</param>
    /// <param name="candidate">The candidate event that matched.</param>
    /// <param name="anchorIndex">Index of the anchor in the input collection.</param>
    /// <param name="candidateIndex">Index of the candidate in the input collection.</param>
    /// <param name="type">The type of match.</param>
    /// <param name="relation">Optional temporal relation (for interval matches).</param>
    void OnMatch(
        in TAnchor anchor,
        in TCandidate candidate,
        int anchorIndex,
        int candidateIndex,
        MatchType type,
        TemporalRelation? relation);

    /// <summary>
    /// Called for each anchor that did not find any matching candidate.
    /// </summary>
    /// <param name="anchor">The anchor event that found no matches.</param>
    /// <param name="anchorIndex">Index of the anchor in the input collection.</param>
    void OnUnmatchedAnchor(in TAnchor anchor, int anchorIndex);
}

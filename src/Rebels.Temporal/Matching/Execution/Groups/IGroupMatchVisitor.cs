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
/// Receives grouped temporal match information where all candidates
/// for each anchor are delivered together in a single callback.
/// </summary>
/// <typeparam name="TAnchor">Type of anchor events.</typeparam>
/// <typeparam name="TCandidate">Type of candidate events.</typeparam>
/// <remarks>
/// <para>
/// Unlike <see cref="IPairMatchVisitor{TAnchor,TCandidate}"/> which calls OnMatch
/// once per matched pair, this visitor receives all matching candidates for
/// an anchor in a single invocation via a <see cref="MatchGroup{TAnchor,TCandidate}"/>.
/// </para>
/// <para>
/// The <see cref="MatchGroup{TAnchor,TCandidate}"/> provides multiple ways to access
/// the matched candidates: via indexer, <see cref="MatchGroup{TAnchor,TCandidate}.Matches"/> span,
/// or <see cref="MatchGroup{TAnchor,TCandidate}.ToArray"/> for creating an owned copy.
/// </para>
/// </remarks>
public interface IGroupMatchVisitor<TAnchor, TCandidate>
{
    /// <summary>
    /// Called once per anchor with all matching candidates grouped together.
    /// </summary>
    /// <param name="group">
    /// The match group containing the anchor and all matched candidates.
    /// </param>
    void OnMatch(in MatchGroup<TAnchor, TCandidate> group);

    /// <summary>
    /// Called exactly once for an anchor that produced no matches.
    /// </summary>
    /// <param name="anchor">The anchor event that had no matching candidates.</param>
    void OnMiss(TAnchor anchor);
}
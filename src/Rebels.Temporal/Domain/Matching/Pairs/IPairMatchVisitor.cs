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
/// Receives temporal match information for anchor–candidate pairs,
/// using an allocation-free visitor pattern.
/// </summary>
/// <typeparam name="TAnchor">Type of anchor events.</typeparam>
/// <typeparam name="TCandidate">Type of candidate events.</typeparam>
public interface IPairMatchVisitor<in TAnchor, in TCandidate>
{
    /// <summary>
    /// Called for every matched pair. The pair includes anchor, candidate,
    /// the matching semantics, and (if applicable) the temporal interval relation.
    /// </summary>
    /// <param name="pair">The match information.</param>
    void OnMatch(in MatchPair<TAnchor, TCandidate> pair);

    /// <summary>
    /// Called exactly once for an anchor that produced no matches.
    /// </summary>
    /// <param name="anchor">The anchor event that had no matching candidates.</param>
    void OnMiss(TAnchor anchor);
}
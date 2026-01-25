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
/// A visitor that writes matches to a user-provided buffer and tracks unmatched anchors.
/// </summary>
/// <typeparam name="TAnchor">Type of anchor events.</typeparam>
/// <typeparam name="TCandidate">Type of candidate events.</typeparam>
/// <remarks>
/// This is a reference implementation equivalent to <see cref="MatchBuffer{TAnchor, TCandidate}"/>
/// but using the visitor pattern. Used for benchmarking visitor vs buffer API performance.
/// </remarks>
public ref struct BufferVisitor<TAnchor, TCandidate> : IMatchVisitor<TAnchor, TCandidate>
{
    private readonly Span<MatchPair<TAnchor, TCandidate>> _pairs;

    /// <summary>
    /// Gets the number of matches found.
    /// </summary>
    public int MatchCount;

    /// <summary>
    /// Gets the number of anchors that did not find any matches.
    /// </summary>
    public int UnmatchedCount;

    /// <summary>
    /// Initializes a new instance with the specified buffer.
    /// </summary>
    public BufferVisitor(Span<MatchPair<TAnchor, TCandidate>> pairs)
    {
        _pairs = pairs;
        MatchCount = 0;
        UnmatchedCount = 0;
    }

    /// <inheritdoc />
    public void OnMatch(
        in TAnchor anchor,
        in TCandidate candidate,
        int anchorIndex,
        int candidateIndex,
        MatchType type,
        TemporalRelation? relation)
    {
        if ((uint)MatchCount >= (uint)_pairs.Length)
            throw new InvalidOperationException(
                "BufferVisitor capacity exceeded. Provide a larger buffer.");

        _pairs[MatchCount++] = new MatchPair<TAnchor, TCandidate>(anchor, candidate, type, relation);
    }

    /// <inheritdoc />
    public void OnUnmatchedAnchor(in TAnchor anchor, int anchorIndex)
    {
        UnmatchedCount++;
    }
}

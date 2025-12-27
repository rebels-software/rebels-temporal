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
/// A ref struct buffer for collecting temporal match results.
/// </summary>
/// <typeparam name="TAnchor">Type of anchor events.</typeparam>
/// <typeparam name="TCandidate">Type of candidate events.</typeparam>
/// <remarks>
/// <para>
/// This buffer is backed by a user-provided span. The matcher will write
/// match results into this buffer and update the <see cref="Count"/> property.
/// </para>
/// <para>
/// Users must ensure the provided buffer is large enough to hold all expected matches.
/// If the buffer capacity is exceeded during matching, an exception will be thrown.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// Span&lt;MatchPair&lt;SensorReading, LogEntry&gt;&gt; bufferSpan = stackalloc MatchPair&lt;SensorReading, LogEntry&gt;[100];
/// var buffer = new MatchBuffer&lt;SensorReading, LogEntry&gt; { Pairs = bufferSpan };
///
/// int matchCount = TemporalMatcher.Match(anchors, candidates, policy, ref buffer);
///
/// for (int i = 0; i &lt; buffer.Count; i++)
/// {
///     var pair = buffer.Pairs[i];
///     // Process match...
/// }
/// </code>
/// </para>
/// </remarks>
public ref struct MatchBuffer<TAnchor, TCandidate>
{
    /// <summary>
    /// Gets or sets the span that will hold match pairs.
    /// </summary>
    public Span<MatchPair<TAnchor, TCandidate>> Pairs;

    /// <summary>
    /// Gets or sets the number of matches currently stored in the buffer.
    /// </summary>
    /// <remarks>
    /// This value is updated by the matcher as matches are found.
    /// After matching completes, this indicates how many valid entries exist in <see cref="Pairs"/>.
    /// </remarks>
    public int Count;

    /// <summary>
    /// Adds a match to the buffer.
    /// </summary>
    /// <param name="anchor">The anchor event.</param>
    /// <param name="candidate">The candidate event that matched.</param>
    /// <param name="type">The type of match.</param>
    /// <param name="relation">Optional temporal relation (for interval matches).</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the buffer capacity is exceeded.
    /// </exception>
    public void Add(in TAnchor anchor, in TCandidate candidate, MatchType type, TemporalRelation? relation = null)
    {
        if ((uint)Count >= (uint)Pairs.Length)
            throw new InvalidOperationException(
                "MatchBuffer capacity exceeded. Provide a larger buffer.");

        Pairs[Count++] = new MatchPair<TAnchor, TCandidate>(
            anchor, candidate, type, relation);
    }
}

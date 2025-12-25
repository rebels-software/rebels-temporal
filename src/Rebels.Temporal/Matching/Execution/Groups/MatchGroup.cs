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
/// Represents a group of candidates that matched a single anchor event.
/// </summary>
/// <typeparam name="TAnchor">Type of anchor events.</typeparam>
/// <typeparam name="TCandidate">Type of candidate events.</typeparam>
/// <remarks>
/// <para>
/// This is a value object containing an anchor and all candidates that matched it
/// during a temporal matching operation. The matched candidates are stored internally
/// and exposed through both array and span accessors.
/// </para>
/// <para>
/// Use the <see cref="Matches"/> property for zero-copy access via <see cref="ReadOnlySpan{T}"/>,
/// or use <see cref="ToArray"/> to create an owned copy of the candidates.
/// </para>
/// </remarks>
public readonly struct MatchGroup<TAnchor, TCandidate>
{
    /// <summary>
    /// The anchor event for which matches were found.
    /// </summary>
    public TAnchor Anchor { get; }

    private readonly TCandidate[] _matches;
    private readonly int _count;

    /// <summary>
    /// Gets the number of matched candidates in this group.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Gets a candidate by index.
    /// </summary>
    /// <param name="index">Zero-based index of the candidate.</param>
    /// <returns>The candidate at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when index is less than 0 or greater than or equal to <see cref="Count"/>.
    /// </exception>
    public TCandidate this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
                throw new IndexOutOfRangeException($"Index {index} is out of range. Valid range: 0 to {_count - 1}.");
            return _matches[index];
        }
    }

    /// <summary>
    /// Gets a read-only span view of the matched candidates.
    /// </summary>
    /// <remarks>
    /// This property provides zero-copy access to the underlying data.
    /// The span is valid for as long as this <see cref="MatchGroup{TAnchor,TCandidate}"/> exists.
    /// </remarks>
    public ReadOnlySpan<TCandidate> Matches => _matches.AsSpan(0, _count);

    /// <summary>
    /// Initializes a new instance of <see cref="MatchGroup{TAnchor,TCandidate}"/>.
    /// </summary>
    /// <param name="anchor">The anchor event.</param>
    /// <param name="matches">Array of matched candidates.</param>
    /// <param name="count">The number of valid matches in the array.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="matches"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="count"/> is negative or exceeds the array length.
    /// </exception>
    internal MatchGroup(TAnchor anchor, TCandidate[] matches, int count)
    {
        if (matches == null)
            throw new ArgumentNullException(nameof(matches), "Matches array cannot be null.");

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), count, "Count cannot be negative.");

        if (count > matches.Length)
            throw new ArgumentOutOfRangeException(nameof(count), count,
                $"Count ({count}) cannot exceed matches array length ({matches.Length}).");

        Anchor = anchor;
        _matches = matches;
        _count = count;
    }

    /// <summary>
    /// Creates an owned copy of the matched candidates as an array.
    /// </summary>
    /// <returns>
    /// A new array containing copies of all matched candidates.
    /// Returns an empty array if there are no matches.
    /// </returns>
    public TCandidate[] ToArray()
    {
        if (_count == 0)
            return Array.Empty<TCandidate>();

        var result = new TCandidate[_count];
        Array.Copy(_matches, 0, result, 0, _count);
        return result;
    }
}

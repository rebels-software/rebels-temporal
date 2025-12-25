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
/// Represents a time tolerance window around a temporal event,
/// defining how far backward and forward matching is allowed.
/// </summary>
/// <remarks>
/// This structure is used to specify tolerance ranges for timestamp matching,
/// allowing events to match even when they are not exactly aligned.
/// </remarks>
public readonly struct TimeTolerance
{
    /// <summary>
    /// Gets the tolerance before the reference timestamp.
    /// </summary>
    /// <remarks>
    /// A positive value means the matcher will look backward in time.
    /// For example, TimeSpan.FromSeconds(5) allows matching events up to 5 seconds earlier.
    /// </remarks>
    public TimeSpan Before { get; }

    /// <summary>
    /// Gets the tolerance after the reference timestamp.
    /// </summary>
    /// <remarks>
    /// A positive value means the matcher will look forward in time.
    /// For example, TimeSpan.FromSeconds(5) allows matching events up to 5 seconds later.
    /// </remarks>
    public TimeSpan After { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeTolerance"/> structure.
    /// </summary>
    /// <param name="before">The tolerance before the reference timestamp.</param>
    /// <param name="after">The tolerance after the reference timestamp.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="before"/> or <paramref name="after"/> is negative.
    /// </exception>
    public TimeTolerance(TimeSpan before, TimeSpan after)
    {
        if (before < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(before), "Before tolerance cannot be negative.");
        }

        if (after < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(after), "After tolerance cannot be negative.");
        }

        Before = before;
        After = after;
    }

    /// <summary>
    /// Creates a symmetric time tolerance with the same value before and after.
    /// </summary>
    /// <param name="tolerance">The tolerance value to apply in both directions.</param>
    /// <returns>A new <see cref="TimeTolerance"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="tolerance"/> is negative.
    /// </exception>
    public static TimeTolerance Symmetric(TimeSpan tolerance)
    {
        return new TimeTolerance(tolerance, tolerance);
    }

    /// <summary>
    /// Gets a zero tolerance (exact matching required).
    /// </summary>
    public static TimeTolerance None => new(TimeSpan.Zero, TimeSpan.Zero);

    /// <summary>
    /// Determines whether this tolerance represents exact matching (no tolerance).
    /// </summary>
    public bool IsExact => Before == TimeSpan.Zero && After == TimeSpan.Zero;
}

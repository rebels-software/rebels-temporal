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
/// Configures the behavior of temporal matching operations.
/// </summary>
/// <remarks>
/// This class provides an immutable configuration API similar to JSON.NET's serialization settings.
/// All modifications return new instances, ensuring thread-safety and predictable behavior.
/// </remarks>
public sealed class MatchOptions
{
    private static readonly MatchOptions DEFAULT = new MatchOptions(TimeTolerance.None, TimeTolerance.None, AllowedRelations.Any);

    /// <summary>
    /// Gets the time tolerance applied to anchor events during matching.
    /// </summary>
    /// <remarks>
    /// Defines how far backward and forward the matcher will search around each anchor timestamp.
    /// Default is <see cref="TimeTolerance.None"/> (exact matching).
    /// </remarks>
    public TimeTolerance AnchorTolerance { get; }

    /// <summary>
    /// Gets the time tolerance applied to candidate events during matching.
    /// </summary>
    /// <remarks>
    /// Defines how far backward and forward candidate events can deviate from their timestamps
    /// and still be considered for matching.
    /// Default is <see cref="TimeTolerance.None"/> (exact matching).
    /// </remarks>
    public TimeTolerance CandidateTolerance { get; }

    /// <summary>
    /// Gets the set of temporal relations that are allowed during interval-based matching.
    /// </summary>
    /// <remarks>
    /// Only applies when matching intervals (types implementing <see cref="ITemporalInterval"/>).
    /// For point-based matching, this setting is ignored.
    /// Default is <see cref="AllowedRelations.Any"/> (all relations allowed).
    /// </remarks>
    public AllowedRelations AllowedTemporalRelations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MatchOptions"/> class.
    /// </summary>
    /// <param name="anchorTolerance">The tolerance for anchor events.</param>
    /// <param name="candidateTolerance">The tolerance for candidate events.</param>
    /// <param name="allowedTemporalRelations">The allowed temporal relations for interval matching.</param>
    public MatchOptions(
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedTemporalRelations)
    {
        AnchorTolerance = anchorTolerance;
        CandidateTolerance = candidateTolerance;
        AllowedTemporalRelations = allowedTemporalRelations;
    }

    /// <summary>
    /// Gets the default match options with exact matching and all relations allowed.
    /// </summary>
    public static MatchOptions Default { get; } = DEFAULT;
}

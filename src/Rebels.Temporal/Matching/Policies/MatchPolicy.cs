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
/// Defines a runtime matching policy used by the temporal matcher
/// to select the optimal matching algorithm and configure matching behavior.
/// </summary>
/// <remarks>
/// <para>
/// A match policy describes:
/// <list type="bullet">
///   <item>how time tolerances are applied to anchors and candidates,</item>
///   <item>which temporal relations are considered valid matches,</item>
///   <item>what ordering guarantees are provided by the input data.</item>
/// </list>
/// </para>
/// <para>
/// Policies can be configured at runtime based on application requirements,
/// configuration files, or dynamic system conditions.
/// </para>
/// </remarks>
public class MatchPolicy
{
    /// <summary>
    /// Gets or sets the time tolerance applied to anchor events.
    /// </summary>
    public TimeTolerance AnchorTolerance { get; set; } = TimeTolerance.None;

    /// <summary>
    /// Gets or sets the time tolerance applied to candidate events.
    /// </summary>
    public TimeTolerance CandidateTolerance { get; set; } = TimeTolerance.None;

    /// <summary>
    /// Gets or sets the set of temporal relations that are considered valid matches.
    /// </summary>
    public AllowedRelations AllowedTemporalRelations { get; set; } = AllowedRelations.Any;

    /// <summary>
    /// Gets or sets which collection is sorted in ascending temporal order.
    /// </summary>
    public InputOrdering InputOrdering { get; set; } = InputOrdering.None;
}

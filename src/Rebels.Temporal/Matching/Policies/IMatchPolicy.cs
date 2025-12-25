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
/// Defines a compile-time matching policy used by the temporal matcher
/// to generate an optimal matching algorithm.
///
/// Implementations of this interface are evaluated at compile time
/// by source generators and must therefore expose all configuration
/// via static members.
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
/// Policies are intended to be stable, design-time decisions.
/// They are not meant to be configured dynamically at runtime.
/// </para>
/// </remarks>
public interface IMatchPolicy
{
    /// <summary>
    /// Gets the time tolerance applied to anchor events.
    /// </summary>
    static abstract TimeTolerance AnchorTolerance { get; }

    /// <summary>
    /// Gets the time tolerance applied to candidate events.
    /// </summary>
    static abstract TimeTolerance CandidateTolerance { get; }

    /// <summary>
    /// Gets the set of temporal relations that are considered valid matches.
    /// </summary>
    static abstract AllowedRelations AllowedTemporalRelations { get; }

    /// <summary>
    /// Indicates which colletion is sorted.
    /// </summary>
    static abstract InputOrdering InputOrdering { get; }
}

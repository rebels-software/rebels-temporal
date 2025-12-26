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

namespace Rebels.Temporal.Tests.TestData;

/// <summary>
/// Exact matching with no tolerance, unsorted input.
/// </summary>
public struct ExactMatchPolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.None;
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.None;
}

/// <summary>
/// Symmetric 5-second window matching, unsorted input.
/// </summary>
public struct SymmetricTolerancePolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.Symmetric(TimeSpan.FromSeconds(5));
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.None;
}

/// <summary>
/// Asymmetric tolerance: 10s before anchor, 5s after anchor, unsorted input.
/// </summary>
public struct AsymmetricTolerancePolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => new TimeTolerance(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5));
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.None;
}

/// <summary>
/// Both anchor and candidate have tolerance, unsorted input.
/// </summary>
public struct BothSidesTolerancePolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.Symmetric(TimeSpan.FromSeconds(3));
    public static TimeTolerance CandidateTolerance => TimeTolerance.Symmetric(TimeSpan.FromSeconds(2));
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.None;
}

/// <summary>
/// Exact matching with sorted candidates (for future optimized algorithms).
/// </summary>
public struct SortedCandidatesPolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.None;
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.Candidates;
}

/// <summary>
/// Both anchors and candidates sorted (for future dual-pointer algorithms).
/// </summary>
public struct BothSortedPolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.None;
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations => AllowedRelations.Any;
    public static InputOrdering InputOrdering => InputOrdering.Both;
}

/// <summary>
/// Filtering specific Allen relations (for interval matching).
/// Only allows: Equal, During, Contains.
/// </summary>
public struct FilteredRelationsPolicy : IMatchPolicy
{
    public static TimeTolerance AnchorTolerance => TimeTolerance.None;
    public static TimeTolerance CandidateTolerance => TimeTolerance.None;
    public static AllowedRelations AllowedTemporalRelations =>
        AllowedRelations.Equal | AllowedRelations.During | AllowedRelations.Contains;
    public static InputOrdering InputOrdering => InputOrdering.None;
}

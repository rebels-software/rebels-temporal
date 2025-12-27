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
/// Provides reusable match policy instances for testing.
/// </summary>
public static class TestPolicies
{
    /// <summary>
    /// Exact matching with no tolerance, unsorted input.
    /// </summary>
    public static MatchPolicy ExactMatch { get; } = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.None,
        CandidateTolerance = TimeTolerance.None,
        AllowedTemporalRelations = AllowedRelations.Any,
        InputOrdering = InputOrdering.None
    };

    /// <summary>
    /// Symmetric 5-second window matching, unsorted input.
    /// </summary>
    public static MatchPolicy SymmetricTolerance { get; } = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(5)),
        CandidateTolerance = TimeTolerance.None,
        AllowedTemporalRelations = AllowedRelations.Any,
        InputOrdering = InputOrdering.None
    };

    /// <summary>
    /// Asymmetric tolerance: 10s before anchor, 5s after anchor, unsorted input.
    /// </summary>
    public static MatchPolicy AsymmetricTolerance { get; } = new MatchPolicy
    {
        AnchorTolerance = new TimeTolerance(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)),
        CandidateTolerance = TimeTolerance.None,
        AllowedTemporalRelations = AllowedRelations.Any,
        InputOrdering = InputOrdering.None
    };

    /// <summary>
    /// Both anchor and candidate have tolerance, unsorted input.
    /// </summary>
    public static MatchPolicy BothSidesTolerance { get; } = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(3)),
        CandidateTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(2)),
        AllowedTemporalRelations = AllowedRelations.Any,
        InputOrdering = InputOrdering.None
    };

    /// <summary>
    /// Exact matching with sorted candidates (for optimized binary search).
    /// </summary>
    public static MatchPolicy SortedCandidates { get; } = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.None,
        CandidateTolerance = TimeTolerance.None,
        AllowedTemporalRelations = AllowedRelations.Any,
        InputOrdering = InputOrdering.Candidates
    };

    /// <summary>
    /// Both anchors and candidates sorted (for dual-pointer scan).
    /// </summary>
    public static MatchPolicy BothSorted { get; } = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.None,
        CandidateTolerance = TimeTolerance.None,
        AllowedTemporalRelations = AllowedRelations.Any,
        InputOrdering = InputOrdering.Both
    };

    /// <summary>
    /// Filtering specific Allen relations (for interval matching).
    /// Only allows: Equal, During, Contains.
    /// </summary>
    public static MatchPolicy FilteredRelations { get; } = new MatchPolicy
    {
        AnchorTolerance = TimeTolerance.None,
        CandidateTolerance = TimeTolerance.None,
        AllowedTemporalRelations = AllowedRelations.Equal | AllowedRelations.During | AllowedRelations.Contains,
        InputOrdering = InputOrdering.None
    };
}

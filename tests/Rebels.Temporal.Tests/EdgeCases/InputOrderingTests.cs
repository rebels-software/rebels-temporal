// Copyright (C) 2026 Rebels Software
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

using Rebels.Temporal.Tests.Reference;
using Rebels.Temporal.Tests.TestData;

namespace Rebels.Temporal.Tests.EdgeCases;

/// <summary>
/// Tests for InputOrdering validation and optimized algorithms.
/// Per INV-10, when ordering is declared, the data must be sorted.
/// </summary>
[TestFixture]
public class InputOrderingTests : MatchingTestBase
{
    #region Sorted Candidates - Binary Search Algorithm

    [Test]
    public void SortedCandidates_Should_Match_Correctly_With_Sorted_Data()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void SortedCandidates_Should_Throw_When_Candidates_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Candidates are not sorted: 20, 10, 0
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(0, 10, 20);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(20, 10, 0);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.SortedCandidates, ref visitor);
        });
    }

    #endregion

    #region Both Sorted - Dual Pointer Algorithm

    [Test]
    public void BothSorted_Should_Match_Correctly_With_Sorted_Data()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSorted)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void BothSorted_Should_Throw_When_Anchors_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Anchors are not sorted: 20, 10, 0
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(20, 10, 0);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(0, 10, 20);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.BothSorted, ref visitor);
        });
    }

    [Test]
    public void BothSorted_Should_Throw_When_Candidates_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Candidates are not sorted: 20, 10, 0
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(0, 10, 20);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(20, 10, 0);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.BothSorted, ref visitor);
        });
    }

    [Test]
    public void BothSorted_Should_Throw_When_Both_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(20, 10, 0);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(30, 20, 10);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.BothSorted, ref visitor);
        });
    }

    #endregion

    #region No Ordering - Unsorted Data Accepted

    [Test]
    public void NoOrdering_Should_Accept_Unsorted_Anchors()
    {
        Given
            .AnchorOffsets(20, 10, 0)
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void NoOrdering_Should_Accept_Unsorted_Candidates()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(20, 10, 0)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void NoOrdering_Should_Accept_Both_Unsorted()
    {
        Given
            .AnchorOffsets(20, 10, 0)
            .CandidateOffsets(30, 20, 10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(2); // 20 matches 20, 10 matches 10
    }

    #endregion

    #region Edge Cases for Ordering

    [Test]
    public void SortedCandidates_Should_Accept_Equal_Consecutive_Values()
    {
        // Equal consecutive values are allowed (non-decreasing order)
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(0, 0, 10, 10, 20, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(6); // Each anchor matches two candidates
    }

    [Test]
    public void BothSorted_Should_Accept_Equal_Consecutive_Values()
    {
        Given
            .AnchorOffsets(0, 0, 10, 10)
            .CandidateOffsets(0, 0, 10, 10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSorted)
        .Then
            .TotalMatchCount(8); // 2x2 + 2x2
    }

    [Test]
    public void SortedCandidates_Should_Work_With_Empty_Collections()
    {
        Given
            .AnchorOffsets()
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void BothSorted_Should_Work_With_Empty_Collections()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets()
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSorted)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void SortedCandidates_Should_Work_With_Single_Element()
    {
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(1);
    }

    #endregion

    #region Sorted Algorithms with Tolerance

    [Test]
    public void SortedCandidates_With_Symmetric_Tolerance_Should_Match_Multiple()
    {
        // Symmetric tolerance of 5s with sorted candidates
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.Candidates
        };

        // Anchor at 10, candidates at 5, 10, 15 (all within ±5s window)
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(5, 10, 15)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void BothSorted_With_Symmetric_Tolerance_Should_Match_Correctly()
    {
        // Symmetric tolerance of 5s with both sorted
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.Both
        };

        // Anchors at 10, 20, 30 with candidates at 12, 22, 32 (all within tolerance)
        Given
            .AnchorOffsets(10, 20, 30)
            .CandidateOffsets(12, 22, 32)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void BothSorted_With_ForwardOnly_Tolerance_Should_Match_Only_Later_Candidates()
    {
        // ForwardOnly tolerance of 5s - only matches candidates after anchor
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.ForwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.Both
        };

        // Anchor at 10, candidates at 8, 10, 12 (only 10 and 12 should match)
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(8, 10, 12)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(2); // 10 and 12 match
    }

    [Test]
    public void BothSorted_With_BackwardOnly_Tolerance_Should_Match_Only_Earlier_Candidates()
    {
        // BackwardOnly tolerance of 5s - only matches candidates before anchor
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.BackwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.Both
        };

        // Anchor at 10, candidates at 6, 8, 10, 12 (only 6, 8, and 10 should match)
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(6, 8, 10, 12)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(3); // 6, 8, and 10 match
    }

    [Test]
    public void SortedCandidates_With_Asymmetric_Tolerance_Should_Match_Asymmetric_Window()
    {
        // Asymmetric tolerance: 10s before, 5s after
        var policy = new MatchPolicy
        {
            AnchorTolerance = new TimeTolerance(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.Candidates
        };

        // Anchor at 20, window is [10, 25]
        // Candidates at 5, 10, 15, 20, 25, 30
        // Matches: 10, 15, 20, 25 (4 matches)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(5, 10, 15, 20, 25, 30)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(4);
    }

    [Test]
    public void BothSorted_With_Tolerance_And_Multiple_Anchors()
    {
        // Multiple anchors with overlapping tolerance windows
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.Symmetric(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.Both
        };

        // Anchors at 10, 12 (windows overlap: [5,15] and [7,17])
        // Candidates at 8, 10, 12, 14
        // Anchor 10 matches: 8, 10, 12, 14 (all within [5,15])
        // Anchor 12 matches: 8, 10, 12, 14 (all within [7,17])
        Given
            .AnchorOffsets(10, 12)
            .CandidateOffsets(8, 10, 12, 14)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(8); // 4 matches per anchor
    }

    #endregion
}

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
/// Tests for asymmetric tolerance and candidate tolerance scenarios.
/// </summary>
[TestFixture]
public class ToleranceTests : MatchingTestBase
{
    #region Asymmetric Anchor Tolerance

    [TestCase(9,  0, Description = "Point at 9 should match with point at 20 when tolerance is [10,5] - before lower boundary")]
    [TestCase(10, 1, Description = "Point at 10 should match with point at 20 when tolerance is [10,5] - exactly at lower boundary")]
    [TestCase(12, 1, Description = "Point at 12 should match with point at 20 when tolerance is [10,5] - within")]
    [TestCase(20, 1, Description = "Point at 20 should match with point at 20 when tolerance is [10,5] - exact anchor")]
    [TestCase(25, 1, Description = "Point at 25 should match with point at 20 when tolerance is [10,5] - exactly at upper boundary")]
    [TestCase(26, 0, Description = "Point at 26 should not match with point at 20 when tolerance is [10,5] - after")]
    public void AsymmetricTolerance_Points_Tests(int candidate, int expected)
    {
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(candidate)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricAnchorTolerance)
        .Then
            .TotalMatchCount(expected);
    }

    #endregion

    #region Asymmetric Tolerance for Point-to-Interval

    [Test]
    public void AsymmetricTolerance_PointToInterval_Should_Expand_Window_Correctly()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Point at 20 becomes window [10, 25]
        // Interval [5, 12] overlaps with window at [10, 12]
        Given
            .AnchorOffsets(20)
            .CandidateIntervals((5, 12))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.AsymmetricAnchorTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void AsymmetricTolerance_PointToInterval_Should_Not_Match_Non_Overlapping()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Point at 20 becomes window [10, 25]
        // Interval [0, 5] does not overlap
        Given
            .AnchorOffsets(20)
            .CandidateIntervals((0, 5))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.AsymmetricAnchorTolerance)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Candidate Tolerance (for Interval-to-Point)

    [Test]
    public void CandidateTolerance_IntervalToPoint_Should_Expand_Candidate_Window()
    {
        // BothSidesTolerance: AnchorTolerance 3s, CandidateTolerance 2s
        // Interval [10, 20]
        // Point at 22, with 2s tolerance becomes [20, 24]
        // Interval [10, 20] should match because interval end (20) == candidate window start (20)
        var policy = TestPolicies.BothSidesTolerance;

        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(22);

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var visitor = new BufferVisitor<TestInterval, TestEvent>(buffer);

        MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
    }

    [Test]
    public void CandidateTolerance_IntervalToPoint_Should_Not_Match_Outside_Expanded_Window()
    {
        // BothSidesTolerance: AnchorTolerance 3s, CandidateTolerance 2s
        // Interval [10, 20]
        // Point at 25, with 2s tolerance becomes [23, 27]
        // Interval [10, 20] does not intersect [23, 27]
        var policy = TestPolicies.BothSidesTolerance;

        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(25);

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var visitor = new BufferVisitor<TestInterval, TestEvent>(buffer);

        MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
    }

    [Test]
    public void CandidateTolerance_IntervalToPoint_Should_Match_Point_Near_Interval_Start()
    {
        // BothSidesTolerance: AnchorTolerance 3s, CandidateTolerance 2s
        // Interval [10, 20]
        // Point at 8, with 2s tolerance becomes [6, 10]
        // Interval start (10) == candidate window end (10)
        var policy = TestPolicies.BothSidesTolerance;

        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(8);

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var visitor = new BufferVisitor<TestInterval, TestEvent>(buffer);

        MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
    }

    #endregion

    #region Both Tolerances Combined (Point-to-Point)

    [Test]
    public void BothSidesTolerance_Should_Expand_Both_Windows()
    {
        // BothSidesTolerance: AnchorTolerance 3s symmetric, CandidateTolerance 2s symmetric
        // Anchor at 10 => window [7, 13]
        // Candidate at 15 => for point-to-point, candidate tolerance is not applied to candidate
        // (Point-to-Point uses only AnchorTolerance for the window)
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(13) // Exactly at boundary
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSidesTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void BothSidesTolerance_PointToPoint_Should_Not_Match_Outside_Anchor_Window()
    {
        // BothSidesTolerance: AnchorTolerance 3s symmetric
        // Anchor at 10 => window [7, 13]
        // Candidate at 14 is outside
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(14)
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSidesTolerance)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region ForwardOnly Tolerance

    [Test]
    public void ForwardOnly_Should_Match_Candidate_After_Anchor()
    {
        // ForwardOnly(5s): anchor=10, candidate=15 → match (exactly at boundary)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.ForwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(15)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void ForwardOnly_Should_Match_Candidate_At_Same_Time()
    {
        // ForwardOnly(5s): anchor=10, candidate=10 → match (same time)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.ForwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(10)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void ForwardOnly_Should_Not_Match_Candidate_Before_Anchor()
    {
        // ForwardOnly(5s): anchor=10, candidate=5 → no match (before anchor)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.ForwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(5)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void ForwardOnly_Should_Not_Match_Candidate_Beyond_Window()
    {
        // ForwardOnly(5s): anchor=10, candidate=16 → no match (beyond window)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.ForwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(16)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region BackwardOnly Tolerance

    [Test]
    public void BackwardOnly_Should_Match_Candidate_Before_Anchor()
    {
        // BackwardOnly(5s): anchor=10, candidate=5 → match (exactly at boundary)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.BackwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(5)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void BackwardOnly_Should_Match_Candidate_At_Same_Time()
    {
        // BackwardOnly(5s): anchor=10, candidate=10 → match (same time)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.BackwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(10)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void BackwardOnly_Should_Not_Match_Candidate_After_Anchor()
    {
        // BackwardOnly(5s): anchor=10, candidate=15 → no match (after anchor)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.BackwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(15)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void BackwardOnly_Should_Not_Match_Candidate_Beyond_Window()
    {
        // BackwardOnly(5s): anchor=10, candidate=4 → no match (beyond window)
        var policy = new MatchPolicy
        {
            AnchorTolerance = TimeTolerance.BackwardOnly(TimeSpan.FromSeconds(5)),
            CandidateTolerance = TimeTolerance.None,
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        Given
            .AnchorOffsets(10)
            .CandidateOffsets(4)
        .When
            .MatchPointToPointIsCalled(policy)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Zero Tolerance Edge Cases

    [Test]
    public void ZeroTolerance_Should_Require_Exact_Match()
    {
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void ZeroTolerance_Should_Not_Match_Adjacent_Times()
    {
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(11)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    #endregion
}

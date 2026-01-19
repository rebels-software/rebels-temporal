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

    [Test]
    public void AsymmetricTolerance_Should_Match_Within_Before_Window()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Anchor at 20, window is [10, 25]
        // Candidate at 12 (within before window)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(12)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void AsymmetricTolerance_Should_Match_Within_After_Window()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Anchor at 20, window is [10, 25]
        // Candidate at 24 (within after window)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(24)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void AsymmetricTolerance_Should_Match_At_Exact_Before_Boundary()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Anchor at 20, window is [10, 25]
        // Candidate at 10 (exactly at before boundary)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void AsymmetricTolerance_Should_Match_At_Exact_After_Boundary()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Anchor at 20, window is [10, 25]
        // Candidate at 25 (exactly at after boundary)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(25)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void AsymmetricTolerance_Should_Not_Match_Outside_Before_Boundary()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Anchor at 20, window is [10, 25]
        // Candidate at 9 (outside before boundary)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(9)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricTolerance)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void AsymmetricTolerance_Should_Not_Match_Outside_After_Boundary()
    {
        // AsymmetricTolerance: 10s before, 5s after
        // Anchor at 20, window is [10, 25]
        // Candidate at 26 (outside after boundary)
        Given
            .AnchorOffsets(20)
            .CandidateOffsets(26)
        .When
            .MatchPointToPointIsCalled(TestPolicies.AsymmetricTolerance)
        .Then
            .TotalMatchCount(0);
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
            .MatchPointToIntervalIsCalled(TestPolicies.AsymmetricTolerance)
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
            .MatchPointToIntervalIsCalled(TestPolicies.AsymmetricTolerance)
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

        var anchors = TestDataGenerator.CreateIntervals((10, 20));
        var candidates = TestDataGenerator.CreatePoints(22);

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var matchBuffer = new MatchBuffer<TestInterval, TestEvent> { Pairs = buffer };

        var count = MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref matchBuffer);

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void CandidateTolerance_IntervalToPoint_Should_Not_Match_Outside_Expanded_Window()
    {
        // BothSidesTolerance: AnchorTolerance 3s, CandidateTolerance 2s
        // Interval [10, 20]
        // Point at 25, with 2s tolerance becomes [23, 27]
        // Interval [10, 20] does not intersect [23, 27]
        var policy = TestPolicies.BothSidesTolerance;

        var anchors = TestDataGenerator.CreateIntervals((10, 20));
        var candidates = TestDataGenerator.CreatePoints(25);

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var matchBuffer = new MatchBuffer<TestInterval, TestEvent> { Pairs = buffer };

        var count = MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref matchBuffer);

        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public void CandidateTolerance_IntervalToPoint_Should_Match_Point_Near_Interval_Start()
    {
        // BothSidesTolerance: AnchorTolerance 3s, CandidateTolerance 2s
        // Interval [10, 20]
        // Point at 8, with 2s tolerance becomes [6, 10]
        // Interval start (10) == candidate window end (10)
        var policy = TestPolicies.BothSidesTolerance;

        var anchors = TestDataGenerator.CreateIntervals((10, 20));
        var candidates = TestDataGenerator.CreatePoints(8);

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var matchBuffer = new MatchBuffer<TestInterval, TestEvent> { Pairs = buffer };

        var count = MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref matchBuffer);

        Assert.That(count, Is.EqualTo(1));
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

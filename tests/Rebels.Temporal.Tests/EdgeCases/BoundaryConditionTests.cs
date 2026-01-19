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
/// Tests for boundary conditions: points at interval Start/End,
/// tolerance window boundaries, and edge cases.
/// </summary>
[TestFixture]
public class BoundaryConditionTests : MatchingTestBase
{
    #region Point at Interval Start/End

    [Test]
    public void PointToInterval_Should_Match_Point_Exactly_At_Interval_Start()
    {
        // Point at 10, Interval [10, 20]
        Given
            .AnchorOffsets(10)
            .CandidateIntervals((10, 20))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void PointToInterval_Should_Match_Point_Exactly_At_Interval_End()
    {
        // Point at 20, Interval [10, 20]
        Given
            .AnchorOffsets(20)
            .CandidateIntervals((10, 20))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void PointToInterval_Should_Not_Match_Point_Just_Before_Interval_Start()
    {
        // Point at 9, Interval [10, 20]
        Given
            .AnchorOffsets(9)
            .CandidateIntervals((10, 20))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void PointToInterval_Should_Not_Match_Point_Just_After_Interval_End()
    {
        // Point at 21, Interval [10, 20]
        Given
            .AnchorOffsets(21)
            .CandidateIntervals((10, 20))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Interval-to-Point Boundary

    [Test]
    public void IntervalToPoint_Should_Match_Point_Exactly_At_Interval_Start()
    {
        // Interval [10, 20], Point at 10
        Given
            .AnchorIntervals((10, 20))
            .CandidateOffsets(10)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void IntervalToPoint_Should_Match_Point_Exactly_At_Interval_End()
    {
        // Interval [10, 20], Point at 20
        Given
            .AnchorIntervals((10, 20))
            .CandidateOffsets(20)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void IntervalToPoint_Should_Not_Match_Point_Just_Before_Interval()
    {
        // Interval [10, 20], Point at 9
        Given
            .AnchorIntervals((10, 20))
            .CandidateOffsets(9)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void IntervalToPoint_Should_Not_Match_Point_Just_After_Interval()
    {
        // Interval [10, 20], Point at 21
        Given
            .AnchorIntervals((10, 20))
            .CandidateOffsets(21)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Point-to-Point Tolerance Boundary

    [Test]
    public void PointToPoint_Should_Match_At_Exact_Tolerance_Boundary_Before()
    {
        // Anchor at 10, Candidate at 5, Tolerance 5s symmetric
        // Window: [5, 15], Candidate at 5 is exactly at lower boundary
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(5)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SymmetricTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void PointToPoint_Should_Match_At_Exact_Tolerance_Boundary_After()
    {
        // Anchor at 10, Candidate at 15, Tolerance 5s symmetric
        // Window: [5, 15], Candidate at 15 is exactly at upper boundary
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(15)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SymmetricTolerance)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void PointToPoint_Should_Not_Match_Just_Outside_Tolerance_Before()
    {
        // Anchor at 10, Candidate at 4, Tolerance 5s symmetric
        // Window: [5, 15], Candidate at 4 is outside
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(4)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SymmetricTolerance)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void PointToPoint_Should_Not_Match_Just_Outside_Tolerance_After()
    {
        // Anchor at 10, Candidate at 16, Tolerance 5s symmetric
        // Window: [5, 15], Candidate at 16 is outside
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(16)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SymmetricTolerance)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Touching Intervals (Meets/MetBy)

    [Test]
    public void IntervalToInterval_Touching_At_Boundary_Should_Match_Meets()
    {
        // Interval [0, 10] touches [10, 20] at point 10
        Given
            .AnchorIntervals((0, 10))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Meets);
    }

    [Test]
    public void IntervalToInterval_Touching_At_Boundary_Should_Match_MetBy()
    {
        // Interval [10, 20] touches [0, 10] at point 10
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((0, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.MetBy);
    }

    #endregion

    #region Multiple Points at Same Timestamp

    [Test]
    public void PointToPoint_Multiple_Points_At_Same_Time_Should_All_Match()
    {
        // 3 anchors at 10, 2 candidates at 10
        Given
            .AnchorOffsets(10, 10, 10)
            .CandidateOffsets(10, 10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(6); // 3 × 2 = 6
    }

    [Test]
    public void PointToInterval_Multiple_Points_At_Interval_Start_Should_All_Match()
    {
        Given
            .AnchorOffsets(10, 10, 10)
            .CandidateIntervals((10, 20))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    #endregion

    #region Negative Offsets (Time Before Base)

    [Test]
    public void PointToPoint_Should_Handle_Negative_Offsets()
    {
        Given
            .AnchorOffsets(-10, 0, 10)
            .CandidateOffsets(-10, 0, 10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void PointToInterval_Should_Handle_Negative_Interval_Offsets()
    {
        // Point at -5, Interval [-10, 0]
        Given
            .AnchorOffsets(-5)
            .CandidateIntervals((-10, 0))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void IntervalToInterval_Should_Handle_Negative_Offsets()
    {
        Given
            .AnchorIntervals((-20, -10))
            .CandidateIntervals((-15, -5))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Overlaps);
    }

    #endregion
}

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
/// Tests for boundary conditions: points at interval Start/End,
/// tolerance window boundaries, and edge cases.
/// </summary>
[TestFixture]
public class BoundaryConditionTests : MatchingTestBase
{
    #region Point at Interval Start/End

    [TestCase(9,  0, Description = "Should not match when offset is earlier than start")]
    [TestCase(10, 1, Description = "Should match point exactly at interval start")]
    [TestCase(15, 1, Description = "Should match point exactly between")]
    [TestCase(20, 1, Description = "Should match point exactly at interval end")]
    [TestCase(21, 0, Description = "Should not match when offset is later than end")]
    public void PointToInterval_TestCases(int anchorOffset, int expectedMatches)
    {
        Given
            .AnchorOffsets(anchorOffset)
            .CandidateIntervals((10, 20))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(expectedMatches);
    }

    #endregion

    #region Interval-to-Point Boundary

    [TestCase(9,  0, Description = "Should not match point with interval, when candidate is before start")]
    [TestCase(10, 1, Description = "Should match point with interval, when candidate is exactly as start")]
    [TestCase(15, 1, Description = "Should match point with interval, when candidate is between start and end")]
    [TestCase(20, 1, Description = "Should match point with interval, when candidate is exactly at end")]
    [TestCase(21, 0, Description = "Should not match point with interval, when candidate is after end")]
    public void IntervalToPoint_TestCases(int candidateOffset, int expectedMatches)
    {
        Given
            .AnchorIntervals((10, 20))
            .CandidateOffsets(candidateOffset)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(expectedMatches);
    }

    #endregion

    #region Point-to-Point Tolerance Boundary


    [TestCase(4,  0, Description = "Should not match point with point when candidate is outside lower boundary")]
    [TestCase(5,  1, Description = "Should match point with point when candidate is exactly at lower boundary")]
    [TestCase(10, 1, Description = "Should match point with point when candidate falls within tolerance")]
    [TestCase(15, 1, Description = "Should match point with point when candidate is exactly at upper boundary")]
    [TestCase(16, 0, Description = "Should not match point with point when candidate is outside upper boundary")]
    public void PointToPoint_Should_Match_At_Exact_Tolerance_Boundary_Before(int candidateOffset, int expectedMatches)
    {
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(candidateOffset)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SymmetricTolerance)
        .Then
            .TotalMatchCount(expectedMatches);
    }

    #endregion

    #region Intervals Boundary

    [TestCase(21, 31, 1, TemporalRelation.Before,       Description = "Anchor [10,20] ends before candidate [21,30]")]
    [TestCase(20, 30, 1, TemporalRelation.Meets,        Description = "Anchor [10,20] touches [20,30] at point 20")]
    [TestCase(17, 27, 1, TemporalRelation.Overlaps,     Description = "Anchor [10,20] starts before and ends inside candidate [17,27]")]
    [TestCase(10, 30, 1, TemporalRelation.Starts,       Description = "Anchor [10,20] starts together and ends after candidate [10,30]")]
    [TestCase(9,  29, 1, TemporalRelation.During,       Description = "Anchor [10,20] starts after and ends before candidate [9,29]")]
    [TestCase(9,  20, 1, TemporalRelation.Finishes,     Description = "Anchor [10,20] starts later but finishes together with candidate [9,20]")]
    [TestCase(10, 20, 1, TemporalRelation.Equal,        Description = "Anchor [10,20] starts and ends at the same time as candidate [10,20]")]
    [TestCase(0,   9, 1, TemporalRelation.After,        Description = "Anchor [10,20] starts and ends after candidate [0,9]")]
    [TestCase(0,  10, 1, TemporalRelation.MetBy,        Description = "Anchor [10,20] starts exactly when candidate [0, 10] ends")]
    [TestCase(5,  15, 1, TemporalRelation.OverlappedBy, Description = "Anchor [10,20] starts inside and ends after candidate [5, 15]")]
    [TestCase(10, 15, 1, TemporalRelation.StartedBy,    Description = "Anchor [10,20] starts together but ends after candidate [10, 15]")]
    [TestCase(12, 17, 1, TemporalRelation.Contains,     Description = "Anchor [10,20] starts before and ends after candidate [12, 17] finishes")]
    [TestCase(12, 20, 1, TemporalRelation.FinishedBy,   Description = "Anchor [10,20] starts before and ends together with candidate [12, 20]")]
    public void IntervalToInterval_TestCases(int candidateStart, int candidateEnd, int expectedMatches, TemporalRelation expectedRelation)
    {
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((candidateStart, candidateEnd))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(expectedMatches)
            .MatchHasRelation(expectedRelation);
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

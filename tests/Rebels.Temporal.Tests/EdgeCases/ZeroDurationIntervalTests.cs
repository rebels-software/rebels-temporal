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
/// Tests for zero-duration intervals where Start == End.
/// Per INV-1, intervals must satisfy Start &lt;= End, so Start == End is valid.
/// </summary>
[TestFixture]
public class ZeroDurationIntervalTests : MatchingTestBase
{
    #region Point-to-Interval with Zero-Duration Interval

    [Test]
    public void PointToInterval_Should_Match_Point_At_Zero_Duration_Interval()
    {
        // Point at time 10, zero-duration interval [10, 10]
        Given
            .AnchorOffsets(10)
            .CandidateIntervals((10, 10))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void PointToInterval_Should_Not_Match_Point_Before_Zero_Duration_Interval()
    {
        // Point at time 5, zero-duration interval [10, 10]
        Given
            .AnchorOffsets(5)
            .CandidateIntervals((10, 10))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void PointToInterval_Should_Not_Match_Point_After_Zero_Duration_Interval()
    {
        // Point at time 15, zero-duration interval [10, 10]
        Given
            .AnchorOffsets(15)
            .CandidateIntervals((10, 10))
        .When
            .MatchPointToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Interval-to-Point with Zero-Duration Interval

    [Test]
    public void IntervalToPoint_Should_Match_Point_At_Zero_Duration_Interval()
    {
        // Zero-duration interval [10, 10], point at time 10
        Given
            .AnchorIntervals((10, 10))
            .CandidateOffsets(10)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1);
    }

    [Test]
    public void IntervalToPoint_Should_Not_Match_Point_Outside_Zero_Duration_Interval()
    {
        // Zero-duration interval [10, 10], point at time 15
        Given
            .AnchorIntervals((10, 10))
            .CandidateOffsets(15)
        .When
            .MatchIntervalToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Interval-to-Interval with Zero-Duration Intervals

    [Test]
    public void IntervalToInterval_Two_Equal_Zero_Duration_Intervals_Should_Match_Equal()
    {
        // Both intervals are [10, 10]
        Given
            .AnchorIntervals((10, 10))
            .CandidateIntervals((10, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Equal);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_Before_Regular_Interval_Should_Match_Before()
    {
        // Zero-duration [5, 5] before interval [10, 20]
        Given
            .AnchorIntervals((5, 5))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Before);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_At_Start_Of_Interval_Should_Match_Starts()
    {
        // Zero-duration [10, 10] at the start of interval [10, 20]
        Given
            .AnchorIntervals((10, 10))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Starts);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_Inside_Interval_Should_Match_During()
    {
        // Zero-duration [15, 15] inside interval [10, 20]
        Given
            .AnchorIntervals((15, 15))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.During);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_At_End_Of_Interval_Should_Match_Finishes()
    {
        // Zero-duration [20, 20] at the end of interval [10, 20]
        Given
            .AnchorIntervals((20, 20))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Finishes);
    }

    [Test]
    public void IntervalToInterval_Regular_Interval_Contains_Zero_Duration_Should_Match_Contains()
    {
        // Interval [10, 20] contains zero-duration [15, 15]
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((15, 15))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Contains);
    }

    [Test]
    public void IntervalToInterval_Two_Different_Zero_Duration_Intervals_Should_Match_Before()
    {
        // Zero-duration [5, 5] vs zero-duration [10, 10]
        Given
            .AnchorIntervals((5, 5))
            .CandidateIntervals((10, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Before);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_After_Regular_Interval_Should_Match_After()
    {
        // Zero-duration [25, 25] after interval [10, 20]
        Given
            .AnchorIntervals((25, 25))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.After);
    }

    #endregion

    #region Meets/MetBy Edge Cases with Zero-Duration Intervals

    [Test]
    public void IntervalToInterval_Regular_Interval_Ending_At_Zero_Duration_Should_Match_FinishedBy()
    {
        // Regular interval [0, 10] with zero-duration [10, 10]
        // Since zero-duration ends at same point as anchor, this is FinishedBy (not Meets)
        // FinishedBy: anchor.End == candidate.End AND candidate.Start > anchor.Start
        Given
            .AnchorIntervals((0, 10))
            .CandidateIntervals((10, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.FinishedBy);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_At_End_Of_Regular_Interval_Should_Match_Finishes()
    {
        // Zero-duration [20, 20] at end of regular interval [10, 20]
        // Finishes: anchor.End == candidate.End AND anchor.Start > candidate.Start
        Given
            .AnchorIntervals((20, 20))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Finishes);
    }

    [Test]
    public void IntervalToInterval_Regular_Interval_StartedBy_Zero_Duration()
    {
        // Regular interval [10, 20] with zero-duration [10, 10] at its start
        // StartedBy: anchor.Start == candidate.Start AND anchor.End > candidate.End
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((10, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.StartedBy);
    }

    [Test]
    public void IntervalToInterval_Two_Adjacent_Zero_Duration_Intervals_With_Gap()
    {
        // Two zero-duration intervals: [5, 5] and [10, 10] with gap between them
        // Should be Before relation
        Given
            .AnchorIntervals((5, 5))
            .CandidateIntervals((10, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Before);
    }

    [Test]
    public void IntervalToInterval_Zero_Duration_Immediately_Before_Another_Zero_Duration()
    {
        // Two zero-duration intervals at same instant: [10, 10] and [10, 10]
        // Should be Equal (same instant)
        Given
            .AnchorIntervals((10, 10))
            .CandidateIntervals((10, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Equal);
    }

    #endregion
}

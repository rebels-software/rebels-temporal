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

using Rebels.Temporal.Tests.TestData;

namespace Rebels.Temporal.Tests.Reference;

[TestFixture]
public class IntervalToIntervalMatchingTests : MatchingTestBase
{
    #region Allen Relations - All 13 Relations

    [Test]
    public void Should_Detect_Before_Relation()
    {
        // Anchor: [0, 10], Candidate: [20, 30]
        // Anchor ends before Candidate starts
        Given
            .AnchorIntervals((0, 10))
            .CandidateIntervals((20, 30))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Before);
    }

    [Test]
    public void Should_Detect_After_Relation()
    {
        // Anchor: [20, 30], Candidate: [0, 10]
        // Anchor starts after Candidate ends
        Given
            .AnchorIntervals((20, 30))
            .CandidateIntervals((0, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.After);
    }

    [Test]
    public void Should_Detect_Meets_Relation()
    {
        // Anchor: [0, 10], Candidate: [10, 20]
        // Anchor ends exactly when Candidate starts
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
    public void Should_Detect_MetBy_Relation()
    {
        // Anchor: [10, 20], Candidate: [0, 10]
        // Anchor starts exactly when Candidate ends
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((0, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.MetBy);
    }

    [Test]
    public void Should_Detect_Overlaps_Relation()
    {
        // Anchor: [0, 15], Candidate: [10, 25]
        // Anchor starts before Candidate and ends inside Candidate
        Given
            .AnchorIntervals((0, 15))
            .CandidateIntervals((10, 25))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Overlaps);
    }

    [Test]
    public void Should_Detect_OverlappedBy_Relation()
    {
        // Anchor: [10, 25], Candidate: [0, 15]
        // Anchor starts inside Candidate and ends after Candidate
        Given
            .AnchorIntervals((10, 25))
            .CandidateIntervals((0, 15))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.OverlappedBy);
    }

    [Test]
    public void Should_Detect_Starts_Relation()
    {
        // Anchor: [0, 10], Candidate: [0, 20]
        // Anchor and Candidate start together, but Anchor ends earlier
        Given
            .AnchorIntervals((0, 10))
            .CandidateIntervals((0, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Starts);
    }

    [Test]
    public void Should_Detect_StartedBy_Relation()
    {
        // Anchor: [0, 20], Candidate: [0, 10]
        // Anchor and Candidate start together, but Anchor ends later
        Given
            .AnchorIntervals((0, 20))
            .CandidateIntervals((0, 10))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.StartedBy);
    }

    [Test]
    public void Should_Detect_During_Relation()
    {
        // Anchor: [5, 15], Candidate: [0, 20]
        // Anchor is completely inside Candidate
        Given
            .AnchorIntervals((5, 15))
            .CandidateIntervals((0, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.During);
    }

    [Test]
    public void Should_Detect_Contains_Relation()
    {
        // Anchor: [0, 20], Candidate: [5, 15]
        // Anchor completely contains Candidate
        Given
            .AnchorIntervals((0, 20))
            .CandidateIntervals((5, 15))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Contains);
    }

    [Test]
    public void Should_Detect_Finishes_Relation()
    {
        // Anchor: [10, 20], Candidate: [0, 20]
        // Anchor ends together with Candidate, but Anchor starts later
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((0, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Finishes);
    }

    [Test]
    public void Should_Detect_FinishedBy_Relation()
    {
        // Anchor: [0, 20], Candidate: [10, 20]
        // Anchor starts before Candidate but ends together
        Given
            .AnchorIntervals((0, 20))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.FinishedBy);
    }

    [Test]
    public void Should_Detect_Equal_Relation()
    {
        // Anchor: [0, 20], Candidate: [0, 20]
        // Anchor and Candidate are identical
        Given
            .AnchorIntervals((0, 20))
            .CandidateIntervals((0, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Equal);
    }

    #endregion

    #region Filtered Relations

    [Test]
    public void FilteredRelations_Should_Only_Match_Allowed_Relations()
    {
        // FilteredRelations policy allows: Equal, During, Contains
        // Test with intervals that have Equal relation
        Given
            .AnchorIntervals((0, 20))
            .CandidateIntervals((0, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Equal);
    }

    [Test]
    public void FilteredRelations_Should_Reject_Disallowed_Relations()
    {
        // FilteredRelations policy allows: Equal, During, Contains
        // Test with intervals that have Before relation (not allowed)
        Given
            .AnchorIntervals((0, 10))
            .CandidateIntervals((20, 30))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void FilteredRelations_Should_Match_During_Relation()
    {
        // FilteredRelations allows During
        Given
            .AnchorIntervals((5, 15))
            .CandidateIntervals((0, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.During);
    }

    [Test]
    public void FilteredRelations_Should_Match_Contains_Relation()
    {
        // FilteredRelations allows Contains
        Given
            .AnchorIntervals((0, 20))
            .CandidateIntervals((5, 15))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Contains);
    }

    [Test]
    public void FilteredRelations_Should_Reject_Overlaps_Relation()
    {
        // FilteredRelations does NOT allow Overlaps
        Given
            .AnchorIntervals((0, 15))
            .CandidateIntervals((10, 25))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Empty Collections

    [Test]
    public void EmptyAnchors_Should_Return_No_Results()
    {
        Given
            .AnchorIntervals()
            .CandidateIntervals((0, 10), (20, 30))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void EmptyCandidates_Should_Return_No_Results()
    {
        Given
            .AnchorIntervals((0, 10), (20, 30))
            .CandidateIntervals()
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void BothEmpty_Should_Return_No_Results()
    {
        Given
            .AnchorIntervals()
            .CandidateIntervals()
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Multiple Matches

    [Test]
    public void Should_Match_All_Pairs_When_All_Relate()
    {
        // All intervals overlap in some way
        Given
            .AnchorIntervals((0, 30))
            .CandidateIntervals((5, 10), (15, 20), (25, 35))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void Should_Return_Cartesian_Product_When_All_Match()
    {
        // 2 anchors × 2 candidates = 4 matches (if all relate)
        Given
            .AnchorIntervals((0, 50), (0, 50))
            .CandidateIntervals((10, 20), (30, 40))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(4);
    }

    #endregion
}

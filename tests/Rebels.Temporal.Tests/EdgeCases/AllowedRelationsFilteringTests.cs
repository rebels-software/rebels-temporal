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
/// Tests for AllowedRelations filtering in interval-to-interval matching.
/// Verifies that only specified Allen relations produce matches.
/// </summary>
[TestFixture]
public class AllowedRelationsFilteringTests : MatchingTestBase
{
    #region FilteredRelations Policy Tests (Equal | During | Contains)

    [Test]
    public void FilteredRelations_Should_Match_Equal()
    {
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Equal);
    }

    [Test]
    public void FilteredRelations_Should_Match_During()
    {
        // Anchor [12, 18] is during candidate [10, 20]
        Given
            .AnchorIntervals((12, 18))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.During);
    }

    [Test]
    public void FilteredRelations_Should_Match_Contains()
    {
        // Anchor [10, 20] contains candidate [12, 18]
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((12, 18))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(1)
            .MatchHasRelation(TemporalRelation.Contains);
    }

    [Test]
    public void FilteredRelations_Should_Not_Match_Before()
    {
        // Anchor [10, 20] is before candidate [30, 40]
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((30, 40))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void FilteredRelations_Should_Not_Match_After()
    {
        // Anchor [30, 40] is after candidate [10, 20]
        Given
            .AnchorIntervals((30, 40))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void FilteredRelations_Should_Not_Match_Overlaps()
    {
        // Anchor [10, 25] overlaps candidate [20, 35]
        Given
            .AnchorIntervals((10, 25))
            .CandidateIntervals((20, 35))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void FilteredRelations_Should_Not_Match_Meets()
    {
        // Anchor [10, 20] meets candidate [20, 30]
        Given
            .AnchorIntervals((10, 20))
            .CandidateIntervals((20, 30))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void FilteredRelations_Should_Not_Match_Starts()
    {
        // Anchor [10, 15] starts candidate [10, 20]
        Given
            .AnchorIntervals((10, 15))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void FilteredRelations_Should_Not_Match_Finishes()
    {
        // Anchor [15, 20] finishes candidate [10, 20]
        Given
            .AnchorIntervals((15, 20))
            .CandidateIntervals((10, 20))
        .When
            .MatchIntervalToIntervalIsCalled(TestPolicies.FilteredRelations)
        .Then
            .TotalMatchCount(0);
    }

    #endregion

    #region Custom Relation Filters

    [Test]
    public void OnlyOverlaps_Should_Match_Overlaps_And_OverlappedBy()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.Overlaps | AllowedRelations.OverlappedBy,
            InputOrdering = InputOrdering.None
        };

        // Anchor [10, 25] overlaps candidate [20, 35]
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 25));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((20, 35));

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
        Assert.That(buffer[0].Relation, Is.EqualTo(TemporalRelation.Overlaps));
    }

    [Test]
    public void OnlyMeets_Should_Not_Match_Overlaps()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.Meets | AllowedRelations.MetBy,
            InputOrdering = InputOrdering.None
        };

        // Anchor [10, 25] overlaps candidate [20, 35] - should not match Meets
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 25));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((20, 35));

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
    }

    [Test]
    public void OnlyEqual_Should_Only_Match_Identical_Intervals()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.Equal,
            InputOrdering = InputOrdering.None
        };

        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals(
            (10, 20),  // Equal - should match
            (10, 25),  // StartedBy - should not match
            (5, 20)    // FinishedBy - should not match
        );

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
        Assert.That(buffer[0].Relation, Is.EqualTo(TemporalRelation.Equal));
    }

    [Test]
    public void AllContainmentRelations_Should_Match_All_Containment_Cases()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.Contains | AllowedRelations.During |
                                       AllowedRelations.Starts | AllowedRelations.StartedBy |
                                       AllowedRelations.Finishes | AllowedRelations.FinishedBy |
                                       AllowedRelations.Equal,
            InputOrdering = InputOrdering.None
        };

        // Anchor [10, 30] with various candidate relationships
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 30));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals(
            (15, 25),  // Contains
            (10, 20),  // StartedBy
            (20, 30),  // FinishedBy
            (10, 30),  // Equal
            (5, 35)    // During (inverse - anchor is during candidate)
        );

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(5));
    }

    [Test]
    public void NoRelations_Should_Match_Nothing()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.None,
            InputOrdering = InputOrdering.None
        };

        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((10, 20)); // Would be Equal

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(1));
    }

    #endregion

    #region ConvertToAllowedRelation Edge Cases

    [Test]
    public void ConvertToAllowedRelation_Should_Throw_For_Invalid_Enum_Value()
    {
        var invalidRelation = (TemporalRelation)999;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MatchTemporal.ConvertToAllowedRelation(invalidRelation));
    }

    #endregion

    #region Specific Relation Filters for Coverage

    [Test]
    public void OnlyMetBy_Should_Match_When_Anchor_Starts_At_Candidate_End()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.MetBy,
            InputOrdering = InputOrdering.None
        };

        // Anchor [20, 30] is MetBy candidate [10, 20]
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((20, 30));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((10, 20));

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
        Assert.That(buffer[0].Relation, Is.EqualTo(TemporalRelation.MetBy));
    }

    [Test]
    public void OnlyOverlappedBy_Should_Match_When_Candidate_Overlaps_Anchor_From_Before()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.OverlappedBy,
            InputOrdering = InputOrdering.None
        };

        // Anchor [20, 35] is OverlappedBy candidate [10, 25]
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((20, 35));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((10, 25));

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
        Assert.That(buffer[0].Relation, Is.EqualTo(TemporalRelation.OverlappedBy));
    }

    [Test]
    public void AnyRelations_Should_Match_All_Intervals_With_Correct_Relations()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        // Test with multiple candidates to verify Any matches everything
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals(
            (0, 5),    // Before (anchor is After)
            (10, 20),  // Equal
            (30, 40)   // After (anchor is Before)
        );

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(3));
        Assert.That(buffer[0].Relation, Is.EqualTo(TemporalRelation.After));
        Assert.That(buffer[1].Relation, Is.EqualTo(TemporalRelation.Equal));
        Assert.That(buffer[2].Relation, Is.EqualTo(TemporalRelation.Before));
    }

    [Test]
    public void AnyRelations_With_Empty_Candidates_Should_Report_Unmatched()
    {
        var policy = new MatchPolicy
        {
            AllowedTemporalRelations = AllowedRelations.Any,
            InputOrdering = InputOrdering.None
        };

        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestInterval> candidates = ReadOnlySpan<TestInterval>.Empty;

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(1));
    }

    #endregion
}

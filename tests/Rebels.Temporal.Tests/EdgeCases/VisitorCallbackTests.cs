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

namespace Rebels.Temporal.Tests.EdgeCases;

/// <summary>
/// Tests for IMatchVisitor callbacks: OnMatch and OnUnmatchedAnchor.
/// Verifies that the visitor pattern correctly tracks matched and unmatched anchors.
/// </summary>
[TestFixture]
public class VisitorCallbackTests
{
    #region OnUnmatchedAnchor Tests

    [Test]
    public void PointToPoint_Should_Report_Unmatched_When_No_Candidates()
    {
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(10, 20, 30);
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(); // Empty

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(3));
    }

    [Test]
    public void PointToPoint_Should_Report_Unmatched_When_No_Match_Found()
    {
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(10, 20, 30);
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(100, 200); // No overlap

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(3));
    }

    [Test]
    public void PointToPoint_Should_Report_Partial_Matches()
    {
        // 3 anchors, only 1 has a matching candidate
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(10, 20, 30);
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(20); // Only matches middle anchor

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(1));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(2));
    }

    [Test]
    public void PointToPoint_All_Matched_Should_Have_Zero_Unmatched()
    {
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(10, 20, 30);
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(10, 20, 30);

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(3));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(0));
    }

    [Test]
    public void PointToPoint_MatchCount_Plus_UnmatchedCount_Equals_AnchorCount_When_OneToOne()
    {
        // With exact match and unique timestamps, each anchor either matches once or not at all
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(10, 20, 30, 40, 50);
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(20, 40, 60); // 2 matches, 3 unmatched

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(2));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(3));
        // Note: MatchCount + UnmatchedCount = 5 = anchors.Length only when 1:1 matching
    }

    #endregion

    #region Unmatched with Intervals

    [Test]
    public void PointToInterval_Should_Report_Unmatched_When_Point_Outside_All_Intervals()
    {
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(5, 25, 35);
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((10, 20)); // Only [10,20]

        var buffer = new MatchPair<TestEvent, TestInterval>[10];
        var visitor = new BufferVisitor<TestEvent, TestInterval>(buffer);

        MatchTemporal.Points.With.Intervals(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0)); // 5, 25, 35 are all outside [10,20]
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(3));
    }

    [Test]
    public void IntervalToPoint_Should_Report_Unmatched_When_Interval_Contains_No_Points()
    {
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20), (30, 40));
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(25); // Between intervals

        var buffer = new MatchPair<TestInterval, TestEvent>[10];
        var visitor = new BufferVisitor<TestInterval, TestEvent>(buffer);

        MatchTemporal.Intervals.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(2));
    }

    [Test]
    public void IntervalToInterval_Should_Report_Unmatched_When_No_Relation_Matches()
    {
        // Two disjoint intervals with FilteredRelations that excludes Before/After
        ReadOnlySpan<TestInterval> anchors = TestDataGenerator.CreateIntervals((10, 20));
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((30, 40)); // Before relation

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var visitor = new BufferVisitor<TestInterval, TestInterval>(buffer);

        // FilteredRelations only allows Equal, During, Contains - not Before
        MatchTemporal.Intervals.With.Intervals(anchors, candidates, TestPolicies.FilteredRelations, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(0));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(1));
    }

    #endregion

    #region Multiple Matches Per Anchor

    [Test]
    public void PointToPoint_Anchor_With_Multiple_Matches_Should_Not_Be_Unmatched()
    {
        // One anchor matches multiple candidates
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(10);
        ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(10, 10, 10); // 3 at same time

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(3)); // 1 anchor × 3 candidates
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(0)); // Anchor found matches
    }

    [Test]
    public void PointToInterval_Anchor_Matching_Multiple_Intervals_Should_Not_Be_Unmatched()
    {
        // Point at 15 falls within two overlapping intervals
        ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(15);
        ReadOnlySpan<TestInterval> candidates = TestDataGenerator.CreateIntervals((10, 20), (12, 18));

        var buffer = new MatchPair<TestEvent, TestInterval>[10];
        var visitor = new BufferVisitor<TestEvent, TestInterval>(buffer);

        MatchTemporal.Points.With.Intervals(anchors, candidates, TestPolicies.ExactMatch, ref visitor);

        Assert.That(visitor.MatchCount, Is.EqualTo(2));
        Assert.That(visitor.UnmatchedCount, Is.EqualTo(0));
    }

    #endregion
}

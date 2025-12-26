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

using Rebels.Temporal.Tests.TestData;

namespace Rebels.Temporal.Tests.Reference;

/// <summary>
/// Chicago-style integration tests for ReferenceTemporalMatcher.
/// These tests validate the reference implementation that serves as source of truth
/// for future source-generated code.
/// </summary>
[TestFixture]
public class ReferenceTemporalMatcherTests
{
    #region Point-to-Point Tests

    [Test]
    public void MatchPointToPoint_ExactPolicy_Should_Match_Exact_Timestamps()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(0, 10, 20, 30);
        var candidates = TestDataGenerator.CreatePoints(10, 20, 40, 50);
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(2));
        Assert.That(visitor.Matches[0].Anchor.Name, Is.EqualTo("Event_10s"));
        Assert.That(visitor.Matches[0].Candidate.Name, Is.EqualTo("Event_10s"));
        Assert.That(visitor.Matches[0].MatchType, Is.EqualTo(MatchType.PointExact));
        Assert.That(visitor.Matches[1].Anchor.Name, Is.EqualTo("Event_20s"));
        Assert.That(visitor.Matches[1].Candidate.Name, Is.EqualTo("Event_20s"));
        Assert.That(visitor.Matches[1].MatchType, Is.EqualTo(MatchType.PointExact));

        Assert.That(visitor.Misses.Count, Is.EqualTo(2));
        Assert.That(visitor.Misses[0].Name, Is.EqualTo("Event_0s"));
        Assert.That(visitor.Misses[1].Name, Is.EqualTo("Event_30s"));
    }

    [Test]
    public void MatchPointToPoint_SymmetricTolerance_Should_Match_Within_Window()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(0, 10, 20);
        var candidates = TestDataGenerator.CreatePoints(2, 12, 25);
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act (5-second tolerance)
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, SymmetricTolerancePolicy>(
            anchors, candidates, visitor);

        // Assert - anchor at 0s matches candidate at 2s, anchor at 10s matches candidates at 12s,
        // anchor at 20s matches candidate at 25s (all within 5s window)
        Assert.That(visitor.Matches.Count, Is.EqualTo(3));
        Assert.That(visitor.Matches[0].Anchor.Name, Is.EqualTo("Event_0s"));
        Assert.That(visitor.Matches[0].Candidate.Name, Is.EqualTo("Event_2s"));
        Assert.That(visitor.Matches[0].MatchType, Is.EqualTo(MatchType.PointInInterval));
        Assert.That(visitor.Matches[1].Anchor.Name, Is.EqualTo("Event_10s"));
        Assert.That(visitor.Matches[1].Candidate.Name, Is.EqualTo("Event_12s"));
        Assert.That(visitor.Matches[2].Anchor.Name, Is.EqualTo("Event_20s"));
        Assert.That(visitor.Matches[2].Candidate.Name, Is.EqualTo("Event_25s"));

        Assert.That(visitor.Misses.Count, Is.EqualTo(0));
    }

    [Test]
    public void MatchPointToPoint_BothSidesTolerance_Should_Use_Allen_Algebra()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(10);
        var candidates = TestDataGenerator.CreatePoints(12);
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act (anchor: 3s tolerance, candidate: 2s tolerance)
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, BothSidesTolerancePolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(1));
        Assert.That(visitor.Matches[0].MatchType, Is.EqualTo(MatchType.Interval));
        Assert.That(visitor.Matches[0].Relation, Is.Not.EqualTo(TemporalRelation.Equal));
    }

    [Test]
    public void MatchPointToPoint_EmptyAnchors_Should_Return_No_Results()
    {
        // Arrange
        var anchors = TestDataGenerator.CreateEmptyPoints();
        var candidates = TestDataGenerator.CreatePoints(0, 10, 20);
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(0));
        Assert.That(visitor.Misses.Count, Is.EqualTo(0));
    }

    [Test]
    public void MatchPointToPoint_EmptyCandidates_Should_Return_All_Misses()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(0, 10, 20);
        var candidates = TestDataGenerator.CreateEmptyPoints();
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(0));
        Assert.That(visitor.Misses.Count, Is.EqualTo(3));
    }

    [Test]
    public void MatchPointToPoint_AllMatch_Should_Return_All_Matches()
    {
        // Arrange
        var (anchors, candidates) = TestDataGenerator.CreateAllMatch(5);
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert - each anchor matches all 5 candidates
        Assert.That(visitor.Matches.Count, Is.EqualTo(25));
        Assert.That(visitor.Misses.Count, Is.EqualTo(0));
    }

    [Test]
    public void MatchPointToPoint_NoMatch_Should_Return_All_Misses()
    {
        // Arrange
        var (anchors, candidates) = TestDataGenerator.CreateNoMatch();
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(0));
        Assert.That(visitor.Misses.Count, Is.EqualTo(3));
    }

    [Test]
    public void MatchPointToPoint_ToleranceBoundary_Should_Match_At_Exact_Boundary()
    {
        // Arrange
        var (anchor, candidates) = TestDataGenerator.CreateToleranceBoundaryPoints(5);
        var anchors = new[] { anchor };
        var visitor = new TestPairVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, SymmetricTolerancePolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(3)); // At boundaries and exact
        Assert.That(visitor.Matches[0].Candidate.Name, Is.EqualTo("AtLowerBoundary"));
        Assert.That(visitor.Matches[1].Candidate.Name, Is.EqualTo("Exact"));
        Assert.That(visitor.Matches[2].Candidate.Name, Is.EqualTo("AtUpperBoundary"));
    }

    #endregion

    #region Point-to-Point Grouped Tests

    [Test]
    public void MatchPointToPointGrouped_ExactPolicy_Should_Group_Matches_By_Anchor()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(0, 10);
        var candidates = TestDataGenerator.CreateCoincidentPoints(3, 10); // 3 candidates at 10s
        var visitor = new TestGroupVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPointGrouped<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Groups.Count, Is.EqualTo(1)); // Only anchor at 10s has matches
        Assert.That(visitor.Groups[0].Anchor.Name, Is.EqualTo("Event_10s"));
        Assert.That(visitor.Groups[0].Count, Is.EqualTo(3));

        Assert.That(visitor.Misses.Count, Is.EqualTo(1)); // Anchor at 0s
        Assert.That(visitor.Misses[0].Name, Is.EqualTo("Event_0s"));
    }

    [Test]
    public void MatchPointToPointGrouped_OneToMany_Should_Create_Single_Group()
    {
        // Arrange
        var (anchors, candidates) = TestDataGenerator.CreateOneToMany();
        var visitor = new TestGroupVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPointGrouped<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Groups.Count, Is.EqualTo(1));
        Assert.That(visitor.Groups[0].Count, Is.EqualTo(5));
        Assert.That(visitor.Misses.Count, Is.EqualTo(0));
    }

    [Test]
    public void MatchPointToPointGrouped_EmptyCandidates_Should_Return_All_Misses()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(0, 10, 20);
        var candidates = TestDataGenerator.CreateEmptyPoints();
        var visitor = new TestGroupVisitor<TestEvent, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchPointToPointGrouped<TestEvent, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Groups.Count, Is.EqualTo(0));
        Assert.That(visitor.Misses.Count, Is.EqualTo(3));
    }

    #endregion

    #region Point-to-Interval Tests

    [Test]
    public void MatchPointToInterval_ExactPolicy_Should_Match_Point_Inside_Interval()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(5, 15, 25);
        var candidates = TestDataGenerator.CreateIntervals((0, 10), (20, 30), (40, 50));
        var visitor = new TestPairVisitor<TestEvent, TestInterval>();

        // Act
        ReferenceTemporalMatcher.MatchPointToInterval<TestEvent, TestInterval, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(2));
        Assert.That(visitor.Matches[0].Anchor.Name, Is.EqualTo("Event_5s"));
        Assert.That(visitor.Matches[0].Candidate.Name, Is.EqualTo("Interval_0s_to_10s"));
        Assert.That(visitor.Matches[1].Anchor.Name, Is.EqualTo("Event_25s"));
        Assert.That(visitor.Matches[1].Candidate.Name, Is.EqualTo("Interval_20s_to_30s"));

        Assert.That(visitor.Misses.Count, Is.EqualTo(1));
        Assert.That(visitor.Misses[0].Name, Is.EqualTo("Event_15s"));
    }

    [Test]
    public void MatchPointToInterval_SymmetricTolerance_Should_Expand_Point_To_Window()
    {
        // Arrange
        var anchors = TestDataGenerator.CreatePoints(15);
        var (_, candidates) = TestDataGenerator.CreatePointToIntervalBoundaries(5);
        var visitor = new TestPairVisitor<TestEvent, TestInterval>();

        // Act (anchor becomes [10, 20] window)
        ReferenceTemporalMatcher.MatchPointToInterval<TestEvent, TestInterval, SymmetricTolerancePolicy>(
            anchors, candidates, visitor);

        // Assert - should match intervals that overlap with [10, 20] window
        Assert.That(visitor.Matches.Count, Is.GreaterThan(0));
    }

    #endregion

    #region Interval-to-Point Tests

    [Test]
    public void MatchIntervalToPoint_ExactPolicy_Should_Match_Point_Inside_Interval()
    {
        // Arrange
        var anchors = TestDataGenerator.CreateIntervals((0, 10), (20, 30));
        var candidates = TestDataGenerator.CreatePoints(5, 15, 25);
        var visitor = new TestPairVisitor<TestInterval, TestEvent>();

        // Act
        ReferenceTemporalMatcher.MatchIntervalToPoint<TestInterval, TestEvent, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(2));
        Assert.That(visitor.Matches[0].Anchor.Name, Is.EqualTo("Interval_0s_to_10s"));
        Assert.That(visitor.Matches[0].Candidate.Name, Is.EqualTo("Event_5s"));
        Assert.That(visitor.Matches[1].Anchor.Name, Is.EqualTo("Interval_20s_to_30s"));
        Assert.That(visitor.Matches[1].Candidate.Name, Is.EqualTo("Event_25s"));

        Assert.That(visitor.Misses.Count, Is.EqualTo(0));
    }

    #endregion

    #region Interval-to-Interval Tests

    [Test]
    public void MatchIntervalToInterval_Should_Match_All_Allen_Relations()
    {
        // Arrange
        var pairs = TestDataGenerator.CreateAllAllenRelations();
        var visitor = new TestPairVisitor<TestInterval, TestInterval>();

        foreach (var (anchor, candidate) in pairs)
        {
            visitor.Clear();
            var anchors = new[] { anchor };
            var candidates = new[] { candidate };

            // Act
            ReferenceTemporalMatcher.MatchIntervalToInterval<TestInterval, TestInterval, ExactMatchPolicy>(
                anchors, candidates, visitor);

            // Assert - should match with correct Allen relation
            Assert.That(visitor.Matches.Count, Is.EqualTo(1),
                $"Expected match for {anchor.Name} and {candidate.Name}");
            Assert.That(visitor.Matches[0].MatchType, Is.EqualTo(MatchType.Interval));
        }
    }

    [Test]
    public void MatchIntervalToInterval_FilteredRelations_Should_Only_Match_Allowed_Relations()
    {
        // Arrange - FilteredRelationsPolicy only allows: Equal, During, Contains
        var allPairs = TestDataGenerator.CreateAllAllenRelations();
        var visitor = new TestPairVisitor<TestInterval, TestInterval>();

        // Act - test each pair individually
        foreach (var (anchor, candidate) in allPairs)
        {
            visitor.Clear();
            var anchors = new[] { anchor };
            var candidates = new[] { candidate };

            ReferenceTemporalMatcher.MatchIntervalToInterval<TestInterval, TestInterval, FilteredRelationsPolicy>(
                anchors, candidates, visitor);

            // Assert - should only have match if relation is Equal, During, or Contains
            var expectedRelation = anchor.Name.Contains("Equal") ? TemporalRelation.Equal :
                                   anchor.Name.Contains("During") ? TemporalRelation.During :
                                   anchor.Name.Contains("Contains") ? TemporalRelation.Contains :
                                   (TemporalRelation?)null;

            if (expectedRelation.HasValue)
            {
                Assert.That(visitor.Matches.Count, Is.EqualTo(1),
                    $"Expected match for {anchor.Name}");
                Assert.That(visitor.Matches[0].Relation, Is.EqualTo(expectedRelation.Value));
            }
            else
            {
                Assert.That(visitor.Matches.Count, Is.EqualTo(0),
                    $"Expected no match for {anchor.Name} (filtered out)");
                Assert.That(visitor.Misses.Count, Is.EqualTo(1));
            }
        }
    }

    [Test]
    public void MatchIntervalToInterval_EqualRelation_Should_Match_Identical_Intervals()
    {
        // Arrange
        var (anchor, candidate) = TestDataGenerator.CreateAllenRelation(TemporalRelation.Equal);
        var anchors = new[] { anchor };
        var candidates = new[] { candidate };
        var visitor = new TestPairVisitor<TestInterval, TestInterval>();

        // Act
        ReferenceTemporalMatcher.MatchIntervalToInterval<TestInterval, TestInterval, ExactMatchPolicy>(
            anchors, candidates, visitor);

        // Assert
        Assert.That(visitor.Matches.Count, Is.EqualTo(1));
        Assert.That(visitor.Matches[0].Relation, Is.EqualTo(TemporalRelation.Equal));
        Assert.That(visitor.Matches[0].MatchType, Is.EqualTo(MatchType.Interval));
    }

    #endregion

    #region Test Helpers

    private class TestPairVisitor<TAnchor, TCandidate> : IPairMatchVisitor<TAnchor, TCandidate>
    {
        public List<MatchPair<TAnchor, TCandidate>> Matches { get; } = new();
        public List<TAnchor> Misses { get; } = new();

        public void OnMatch(in MatchPair<TAnchor, TCandidate> pair)
        {
            Matches.Add(pair);
        }

        public void OnMiss(TAnchor anchor)
        {
            Misses.Add(anchor);
        }

        public void Clear()
        {
            Matches.Clear();
            Misses.Clear();
        }
    }

    private class TestGroupVisitor<TAnchor, TCandidate> : IGroupMatchVisitor<TAnchor, TCandidate>
    {
        public List<MatchGroup<TAnchor, TCandidate>> Groups { get; } = new();
        public List<TAnchor> Misses { get; } = new();

        public void OnMatch(in MatchGroup<TAnchor, TCandidate> group)
        {
            Groups.Add(group);
        }

        public void OnMiss(TAnchor anchor)
        {
            Misses.Add(anchor);
        }

        public void Clear()
        {
            Groups.Clear();
            Misses.Clear();
        }
    }

    #endregion
}

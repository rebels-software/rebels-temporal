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

namespace Rebels.Temporal.Tests.Domain.Matching.Pairs;

[TestFixture]
public class MatchPairTests
{
    [Test]
    public void PointExact_Should_Store_Anchor_Candidate_And_MatchType()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidate = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Candidate");

        // Act
        var pair = new MatchPair<TestEvent, TestEvent>(anchor, candidate, MatchType.PointExact);

        // Assert
        Assert.That(pair.Anchor, Is.EqualTo(anchor));
        Assert.That(pair.Candidate, Is.EqualTo(candidate));
        Assert.That(pair.MatchType, Is.EqualTo(MatchType.PointExact));
        Assert.That(pair.Relation, Is.Null);
    }

    [Test]
    public void PointInInterval_Should_Store_Anchor_Candidate_And_MatchType()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 30, TimeSpan.Zero), "Event");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 1, 0, TimeSpan.Zero),
            "Interval");

        // Act
        var pair = new MatchPair<TestEvent, TestInterval>(anchor, candidate, MatchType.PointInInterval);

        // Assert
        Assert.That(pair.Anchor, Is.EqualTo(anchor));
        Assert.That(pair.Candidate, Is.EqualTo(candidate));
        Assert.That(pair.MatchType, Is.EqualTo(MatchType.PointInInterval));
        Assert.That(pair.Relation, Is.Null);
    }

    [Test]
    public void IntervalOverlap_Should_Store_All_Properties_Including_Relation()
    {
        // Arrange
        var anchor = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 30, TimeSpan.Zero),
            "Anchor");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 15, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 45, TimeSpan.Zero),
            "Candidate");

        // Act
        var pair = new MatchPair<TestInterval, TestInterval>(
            anchor,
            candidate,
            MatchType.IntervalOverlap,
            TemporalRelation.Overlaps);

        // Assert
        Assert.That(pair.Anchor, Is.EqualTo(anchor));
        Assert.That(pair.Candidate, Is.EqualTo(candidate));
        Assert.That(pair.MatchType, Is.EqualTo(MatchType.IntervalOverlap));
        Assert.That(pair.Relation, Is.EqualTo(TemporalRelation.Overlaps));
    }

    [TestCase(TemporalRelation.Before)]
    [TestCase(TemporalRelation.Meets)]
    [TestCase(TemporalRelation.Overlaps)]
    [TestCase(TemporalRelation.Starts)]
    [TestCase(TemporalRelation.During)]
    [TestCase(TemporalRelation.Finishes)]
    [TestCase(TemporalRelation.Equal)]
    [TestCase(TemporalRelation.After)]
    [TestCase(TemporalRelation.MetBy)]
    [TestCase(TemporalRelation.OverlappedBy)]
    [TestCase(TemporalRelation.StartedBy)]
    [TestCase(TemporalRelation.Contains)]
    [TestCase(TemporalRelation.FinishedBy)]
    public void IntervalOverlap_Should_Support_All_Allen_Relations(TemporalRelation relation)
    {
        // Arrange
        var anchor = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 30, TimeSpan.Zero),
            "Anchor");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 15, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 45, TimeSpan.Zero),
            "Candidate");

        // Act
        var pair = new MatchPair<TestInterval, TestInterval>(
            anchor,
            candidate,
            MatchType.IntervalOverlap,
            relation);

        // Assert
        Assert.That(pair.Relation, Is.EqualTo(relation));
    }

    [Test]
    public void PointExact_With_Relation_Should_Throw_ArgumentException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidate = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Candidate");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestEvent, TestEvent>(anchor, candidate, MatchType.PointExact, TemporalRelation.Equal));

        Assert.That(exception!.Message, Does.Contain("PointExact"));
        Assert.That(exception.Message, Does.Contain("cannot have a temporal relation"));
        Assert.That(exception.ParamName, Is.EqualTo("relation"));
    }

    [Test]
    public void PointInInterval_With_Relation_Should_Throw_ArgumentException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 30, TimeSpan.Zero), "Event");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 1, 0, TimeSpan.Zero),
            "Interval");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestEvent, TestInterval>(anchor, candidate, MatchType.PointInInterval, TemporalRelation.During));

        Assert.That(exception!.Message, Does.Contain("PointInInterval"));
        Assert.That(exception.Message, Does.Contain("cannot have a temporal relation"));
        Assert.That(exception.ParamName, Is.EqualTo("relation"));
    }

    [Test]
    public void IntervalOverlap_Without_Relation_Should_Throw_ArgumentException()
    {
        // Arrange
        var anchor = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 30, TimeSpan.Zero),
            "Anchor");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 15, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 45, TimeSpan.Zero),
            "Candidate");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestInterval, TestInterval>(anchor, candidate, MatchType.IntervalOverlap));

        Assert.That(exception!.Message, Does.Contain("IntervalOverlap"));
        Assert.That(exception.Message, Does.Contain("requires a temporal relation"));
        Assert.That(exception.ParamName, Is.EqualTo("relation"));
    }

    [Test]
    public void IntervalOverlap_With_Null_Relation_Should_Throw_ArgumentException()
    {
        // Arrange
        var anchor = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 30, TimeSpan.Zero),
            "Anchor");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 15, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 0, 45, TimeSpan.Zero),
            "Candidate");

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestInterval, TestInterval>(anchor, candidate, MatchType.IntervalOverlap, null));

        Assert.That(exception!.Message, Does.Contain("IntervalOverlap"));
        Assert.That(exception.Message, Does.Contain("requires a temporal relation"));
        Assert.That(exception.ParamName, Is.EqualTo("relation"));
    }

    [Test]
    public void MatchPair_Should_Support_Different_Generic_Types()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Event");
        var candidate = new TestInterval(
            new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2025, 1, 1, 12, 1, 0, TimeSpan.Zero),
            "Interval");

        // Act
        var pair = new MatchPair<TestEvent, TestInterval>(anchor, candidate, MatchType.PointInInterval);

        // Assert
        Assert.That(pair.Anchor.Name, Is.EqualTo("Event"));
        Assert.That(pair.Candidate.Name, Is.EqualTo("Interval"));
    }

    [Test]
    public void MatchPair_Should_Be_Readonly_Struct()
    {
        // This is a compile-time check, but we can verify the type is indeed a struct
        var type = typeof(MatchPair<TestEvent, TestEvent>);

        Assert.That(type.IsValueType, Is.True);
    }
}

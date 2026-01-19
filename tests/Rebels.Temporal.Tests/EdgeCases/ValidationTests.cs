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

namespace Rebels.Temporal.Tests.EdgeCases;

/// <summary>
/// Tests for validation: invalid intervals (INV-1), buffer overflow, MatchPair invariants (INV-8).
/// </summary>
[TestFixture]
public class ValidationTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

    #region Invalid Interval Validation (INV-1: Start <= End)

    [Test]
    public void PointToInterval_Should_Throw_When_Candidate_Interval_Is_Invalid()
    {
        // Invalid interval: Start > End
        var anchors = new[] { new TestEvent(BaseTime, "Point") };
        var candidates = new[] { new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(10), "Invalid") };
        var policy = TestPolicies.ExactMatch;

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var buffer = new MatchPair<TestEvent, TestInterval>[10];
            var matchBuffer = new MatchBuffer<TestEvent, TestInterval> { Pairs = buffer };
            MatchTemporal.Points.With.Intervals(anchors, candidates, policy, ref matchBuffer);
        });

        Assert.That(ex!.Message, Does.Contain("Start"));
        Assert.That(ex.Message, Does.Contain("End"));
    }

    [Test]
    public void IntervalToPoint_Should_Throw_When_Anchor_Interval_Is_Invalid()
    {
        // Invalid interval: Start > End
        var anchors = new[] { new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(10), "Invalid") };
        var candidates = new[] { new TestEvent(BaseTime, "Point") };
        var policy = TestPolicies.ExactMatch;

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var buffer = new MatchPair<TestInterval, TestEvent>[10];
            var matchBuffer = new MatchBuffer<TestInterval, TestEvent> { Pairs = buffer };
            MatchTemporal.Intervals.With.Points(anchors, candidates, policy, ref matchBuffer);
        });

        Assert.That(ex!.Message, Does.Contain("Start"));
        Assert.That(ex.Message, Does.Contain("End"));
    }

    [Test]
    public void IntervalToInterval_Should_Throw_When_Anchor_Interval_Is_Invalid()
    {
        var anchors = new[] { new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(10), "Invalid") };
        var candidates = new[] { new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Valid") };
        var policy = TestPolicies.ExactMatch;

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var buffer = new MatchPair<TestInterval, TestInterval>[10];
            var matchBuffer = new MatchBuffer<TestInterval, TestInterval> { Pairs = buffer };
            MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref matchBuffer);
        });

        Assert.That(ex!.Message, Does.Contain("Start"));
    }

    [Test]
    public void IntervalToInterval_Should_Throw_When_Candidate_Interval_Is_Invalid()
    {
        var anchors = new[] { new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Valid") };
        var candidates = new[] { new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(10), "Invalid") };
        var policy = TestPolicies.ExactMatch;

        var ex = Assert.Throws<ArgumentException>(() =>
        {
            var buffer = new MatchPair<TestInterval, TestInterval>[10];
            var matchBuffer = new MatchBuffer<TestInterval, TestInterval> { Pairs = buffer };
            MatchTemporal.Intervals.With.Intervals(anchors, candidates, policy, ref matchBuffer);
        });

        Assert.That(ex!.Message, Does.Contain("Start"));
    }

    #endregion

    #region Buffer Overflow

    [Test]
    public void PointToPoint_Should_Throw_When_Buffer_Is_Too_Small()
    {
        // 3 anchors × 3 candidates = 9 potential matches, but buffer is only 5
        var anchors = TestDataGenerator.CreatePoints(0, 0, 0);
        var candidates = TestDataGenerator.CreatePoints(0, 0, 0);

        Assert.Throws<InvalidOperationException>(() =>
        {
            var buffer = new MatchPair<TestEvent, TestEvent>[5]; // Too small!
            var matchBuffer = new MatchBuffer<TestEvent, TestEvent> { Pairs = buffer };
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);
        });
    }

    [Test]
    public void PointToInterval_Should_Throw_When_Buffer_Is_Too_Small()
    {
        var anchors = TestDataGenerator.CreatePoints(5, 5, 5);
        var candidates = TestDataGenerator.CreateIntervals((0, 10), (0, 10), (0, 10));

        Assert.Throws<InvalidOperationException>(() =>
        {
            var buffer = new MatchPair<TestEvent, TestInterval>[5]; // Too small for 9 matches
            var matchBuffer = new MatchBuffer<TestEvent, TestInterval> { Pairs = buffer };
            MatchTemporal.Points.With.Intervals(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);
        });
    }

    [Test]
    public void IntervalToInterval_Should_Throw_When_Buffer_Is_Too_Small()
    {
        var anchors = TestDataGenerator.CreateIntervals((0, 20), (0, 20), (0, 20));
        var candidates = TestDataGenerator.CreateIntervals((5, 15), (5, 15), (5, 15));

        Assert.Throws<InvalidOperationException>(() =>
        {
            var buffer = new MatchPair<TestInterval, TestInterval>[5]; // Too small for 9 matches
            var matchBuffer = new MatchBuffer<TestInterval, TestInterval> { Pairs = buffer };
            MatchTemporal.Intervals.With.Intervals(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);
        });
    }

    [Test]
    public void Buffer_Exactly_Right_Size_Should_Not_Throw()
    {
        // 2 anchors × 2 candidates = 4 matches, buffer is exactly 4
        var anchors = TestDataGenerator.CreatePoints(0, 0);
        var candidates = TestDataGenerator.CreatePoints(0, 0);

        var buffer = new MatchPair<TestEvent, TestEvent>[4];
        var matchBuffer = new MatchBuffer<TestEvent, TestEvent> { Pairs = buffer };

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);

        Assert.That(matchBuffer.Count, Is.EqualTo(4));
    }

    #endregion

    #region MatchPair Invariants (INV-8)

    [Test]
    public void MatchPair_PointExact_Should_Not_Have_Relation()
    {
        var anchors = TestDataGenerator.CreatePoints(10);
        var candidates = TestDataGenerator.CreatePoints(10);

        var buffer = new MatchPair<TestEvent, TestEvent>[10];
        var matchBuffer = new MatchBuffer<TestEvent, TestEvent> { Pairs = buffer };

        MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);

        Assert.That(matchBuffer.Count, Is.EqualTo(1));
        Assert.That(buffer[0].MatchType, Is.EqualTo(MatchType.PointExact));
        Assert.That(buffer[0].Relation, Is.Null);
    }

    [Test]
    public void MatchPair_PointInInterval_Should_Not_Have_Relation()
    {
        var anchors = TestDataGenerator.CreatePoints(15);
        var candidates = TestDataGenerator.CreateIntervals((10, 20));

        var buffer = new MatchPair<TestEvent, TestInterval>[10];
        var matchBuffer = new MatchBuffer<TestEvent, TestInterval> { Pairs = buffer };

        MatchTemporal.Points.With.Intervals(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);

        Assert.That(matchBuffer.Count, Is.EqualTo(1));
        Assert.That(buffer[0].MatchType, Is.EqualTo(MatchType.PointInInterval));
        Assert.That(buffer[0].Relation, Is.Null);
    }

    [Test]
    public void MatchPair_Interval_Should_Always_Have_Relation()
    {
        var anchors = TestDataGenerator.CreateIntervals((0, 20));
        var candidates = TestDataGenerator.CreateIntervals((5, 15));

        var buffer = new MatchPair<TestInterval, TestInterval>[10];
        var matchBuffer = new MatchBuffer<TestInterval, TestInterval> { Pairs = buffer };

        MatchTemporal.Intervals.With.Intervals(anchors, candidates, TestPolicies.ExactMatch, ref matchBuffer);

        Assert.That(matchBuffer.Count, Is.EqualTo(1));
        Assert.That(buffer[0].MatchType, Is.EqualTo(MatchType.Interval));
        Assert.That(buffer[0].Relation, Is.Not.Null);
        Assert.That(buffer[0].Relation, Is.EqualTo(TemporalRelation.Contains));
    }

    [Test]
    public void MatchPair_Constructor_Should_Throw_When_PointExact_Has_Relation()
    {
        var anchor = new TestEvent(BaseTime, "Anchor");
        var candidate = new TestEvent(BaseTime, "Candidate");

        Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestEvent, TestEvent>(anchor, candidate, MatchType.PointExact, TemporalRelation.Equal));
    }

    [Test]
    public void MatchPair_Constructor_Should_Throw_When_PointInInterval_Has_Relation()
    {
        var anchor = new TestEvent(BaseTime, "Anchor");
        var candidate = new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Candidate");

        Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestEvent, TestInterval>(anchor, candidate, MatchType.PointInInterval, TemporalRelation.During));
    }

    [Test]
    public void MatchPair_Constructor_Should_Throw_When_Interval_Has_No_Relation()
    {
        var anchor = new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Anchor");
        var candidate = new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Candidate");

        Assert.Throws<ArgumentException>(() =>
            new MatchPair<TestInterval, TestInterval>(anchor, candidate, MatchType.Interval, null));
    }

    [Test]
    public void MatchPair_Constructor_Should_Accept_Valid_PointExact_Without_Relation()
    {
        var anchor = new TestEvent(BaseTime, "Anchor");
        var candidate = new TestEvent(BaseTime, "Candidate");

        var pair = new MatchPair<TestEvent, TestEvent>(anchor, candidate, MatchType.PointExact, null);

        Assert.That(pair.MatchType, Is.EqualTo(MatchType.PointExact));
        Assert.That(pair.Relation, Is.Null);
    }

    [Test]
    public void MatchPair_Constructor_Should_Accept_Valid_Interval_With_Relation()
    {
        var anchor = new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Anchor");
        var candidate = new TestInterval(BaseTime, BaseTime.AddSeconds(10), "Candidate");

        var pair = new MatchPair<TestInterval, TestInterval>(anchor, candidate, MatchType.Interval, TemporalRelation.Equal);

        Assert.That(pair.MatchType, Is.EqualTo(MatchType.Interval));
        Assert.That(pair.Relation, Is.EqualTo(TemporalRelation.Equal));
    }

    #endregion
}

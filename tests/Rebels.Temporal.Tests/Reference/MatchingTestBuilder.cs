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

using NUnit.Framework.Constraints;
using Rebels.Temporal.Tests.TestData;

namespace Rebels.Temporal.Tests.Reference;

/// <summary>
/// Fluent builder for BDD-style temporal matching tests.
/// </summary>
public class MatchingTestBuilder
{
    private TestEvent[]? _anchors;
    private TestEvent[]? _candidates;
    private TestInterval[]? _anchorIntervals;
    private TestInterval[]? _candidateIntervals;
    private MatchPair<TestEvent, TestEvent>[]? _pointMatches;
    private MatchPair<TestEvent, TestInterval>[]? _pointToIntervalMatches;
    private MatchPair<TestInterval, TestEvent>[]? _intervalToPointMatches;
    private MatchPair<TestInterval, TestInterval>[]? _intervalMatches;
    private int _matchCount;
    private int _unmatchedCount;

    public static MatchingTestBuilder Given => new();

    public MatchingTestBuilder AnchorOffsets(params int[] offsetsInSeconds)
    {
        _anchors = TestDataGenerator.CreatePoints(offsetsInSeconds);
        return this;
    }

    public MatchingTestBuilder CandidateOffsets(params int[] offsetsInSeconds)
    {
        _candidates = TestDataGenerator.CreatePoints(offsetsInSeconds);
        return this;
    }

    public MatchingTestBuilder AnchorIntervals(params (int start, int end)[] intervals)
    {
        _anchorIntervals = TestDataGenerator.CreateIntervals(intervals);
        return this;
    }

    public MatchingTestBuilder CandidateIntervals(params (int start, int end)[] intervals)
    {
        _candidateIntervals = TestDataGenerator.CreateIntervals(intervals);
        return this;
    }

    public MatchingTestBuilder When => this;

    public MatchingTestBuilder MatchPointToPointIsCalled(MatchPolicy policy)
    {
        var anchors = _anchors ?? Array.Empty<TestEvent>();
        var candidates = _candidates ?? Array.Empty<TestEvent>();

        // Allocate buffer large enough for all possible matches
        var maxMatches = anchors.Length * candidates.Length;
        _pointMatches = new MatchPair<TestEvent, TestEvent>[Math.Max(1, maxMatches)];

        var visitor = new BufferVisitor<TestEvent, TestEvent>(_pointMatches);
        MatchTemporal.Points.With.Points<TestEvent, TestEvent, BufferVisitor<TestEvent, TestEvent>>(
            anchors, candidates, policy, ref visitor);

        _matchCount = visitor.MatchCount;
        _unmatchedCount = visitor.UnmatchedCount;

        return this;
    }

    public MatchingTestBuilder MatchPointToIntervalIsCalled(MatchPolicy policy)
    {
        var anchors = _anchors ?? Array.Empty<TestEvent>();
        var candidates = _candidateIntervals ?? Array.Empty<TestInterval>();

        var maxMatches = anchors.Length * candidates.Length;
        _pointToIntervalMatches = new MatchPair<TestEvent, TestInterval>[Math.Max(1, maxMatches)];

        var visitor = new BufferVisitor<TestEvent, TestInterval>(_pointToIntervalMatches);
        MatchTemporal.Points.With.Intervals<TestEvent, TestInterval, BufferVisitor<TestEvent, TestInterval>>(
            anchors, candidates, policy, ref visitor);

        _matchCount = visitor.MatchCount;
        _unmatchedCount = visitor.UnmatchedCount;

        return this;
    }

    public MatchingTestBuilder MatchIntervalToPointIsCalled(MatchPolicy policy)
    {
        var anchors = _anchorIntervals ?? Array.Empty<TestInterval>();
        var candidates = _candidates ?? Array.Empty<TestEvent>();

        var maxMatches = anchors.Length * candidates.Length;
        _intervalToPointMatches = new MatchPair<TestInterval, TestEvent>[Math.Max(1, maxMatches)];

        var visitor = new BufferVisitor<TestInterval, TestEvent>(_intervalToPointMatches);
        MatchTemporal.Intervals.With.Points<TestInterval, TestEvent, BufferVisitor<TestInterval, TestEvent>>(
            anchors, candidates, policy, ref visitor);

        _matchCount = visitor.MatchCount;
        _unmatchedCount = visitor.UnmatchedCount;

        return this;
    }

    public MatchingTestBuilder MatchIntervalToIntervalIsCalled(MatchPolicy policy)
    {
        var anchors = _anchorIntervals ?? Array.Empty<TestInterval>();
        var candidates = _candidateIntervals ?? Array.Empty<TestInterval>();

        var maxMatches = anchors.Length * candidates.Length;
        _intervalMatches = new MatchPair<TestInterval, TestInterval>[Math.Max(1, maxMatches)];

        var visitor = new BufferVisitor<TestInterval, TestInterval>(_intervalMatches);
        MatchTemporal.Intervals.With.Intervals<TestInterval, TestInterval, BufferVisitor<TestInterval, TestInterval>>(
            anchors, candidates, policy, ref visitor);

        _matchCount = visitor.MatchCount;
        _unmatchedCount = visitor.UnmatchedCount;

        return this;
    }

    public MatchingTestBuilder Then => this;

    public MatchingTestBuilder PairsAreFound(MatchType matchType, params (int anchorOffset, int candidateOffset)[] expectedPairs)
    {
        if (_pointMatches != null)
        {
            Assert.That(_matchCount, Is.EqualTo(expectedPairs.Length));
            for (int i = 0; i < expectedPairs.Length; i++)
            {
                var (anchorOffset, candidateOffset) = expectedPairs[i];
                Assert.That(_pointMatches[i].Anchor.Name, Is.EqualTo($"Event_{anchorOffset}s"));
                Assert.That(_pointMatches[i].Candidate.Name, Is.EqualTo($"Event_{candidateOffset}s"));
                Assert.That(_pointMatches[i].MatchType, Is.EqualTo(matchType));
            }
        }
        return this;
    }

    public MatchingTestBuilder IntervalPairsAreFound(params (int anchorStart, int anchorEnd, int candidateStart, int candidateEnd)[] expectedPairs)
    {
        if (_intervalMatches != null)
        {
            Assert.That(_matchCount, Is.EqualTo(expectedPairs.Length));
            for (int i = 0; i < expectedPairs.Length; i++)
            {
                var (anchorStart, anchorEnd, candidateStart, candidateEnd) = expectedPairs[i];
                Assert.That(_intervalMatches[i].Anchor.Name, Is.EqualTo($"Interval_{anchorStart}s_to_{anchorEnd}s"));
                Assert.That(_intervalMatches[i].Candidate.Name, Is.EqualTo($"Interval_{candidateStart}s_to_{candidateEnd}s"));
                Assert.That(_intervalMatches[i].MatchType, Is.EqualTo(MatchType.Interval));
            }
        }
        return this;
    }

    public MatchingTestBuilder UnmatchedAnchors(params int[] offsetsInSeconds)
    {
        // Verify unmatched count matches expected
        Assert.That(_unmatchedCount, Is.EqualTo(offsetsInSeconds.Length));
        return this;
    }

    public MatchingTestBuilder TotalMatchCount(int count)
    {
        Assert.That(_matchCount, Is.EqualTo(count));
        return this;
    }

    public MatchingTestBuilder TotalMissCount(int count)
    {
        Assert.That(_unmatchedCount, Is.EqualTo(count));
        return this;
    }

    public MatchingTestBuilder TotalMatchCount(IResolveConstraint constraint)
    {
        Assert.That(_matchCount, constraint);
        return this;
    }

    public MatchingTestBuilder AllMatchesHaveType(MatchType expectedType)
    {
        if (_intervalMatches != null)
        {
            for (int i = 0; i < _matchCount; i++)
            {
                Assert.That(_intervalMatches[i].MatchType, Is.EqualTo(expectedType));
            }
        }
        return this;
    }

    public MatchingTestBuilder MatchHasRelation(TemporalRelation expectedRelation)
    {
        if (_intervalMatches != null && _matchCount > 0)
        {
            Assert.That(_intervalMatches[0].Relation, Is.EqualTo(expectedRelation));
        }
        return this;
    }
}

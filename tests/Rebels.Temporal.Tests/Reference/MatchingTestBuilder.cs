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
    private TestPairVisitor<TestEvent, TestEvent>? _pointVisitor;
    private TestPairVisitor<TestEvent, TestInterval>? _pointToIntervalVisitor;
    private TestPairVisitor<TestInterval, TestEvent>? _intervalToPointVisitor;
    private TestPairVisitor<TestInterval, TestInterval>? _intervalVisitor;
    private TestGroupVisitor<TestEvent, TestEvent>? _pointGroupVisitor;
    private TestGroupVisitor<TestEvent, TestInterval>? _pointToIntervalGroupVisitor;
    private TestGroupVisitor<TestInterval, TestEvent>? _intervalToPointGroupVisitor;
    private TestGroupVisitor<TestInterval, TestInterval>? _intervalGroupVisitor;

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

    public MatchingTestBuilder MatchPointToPointIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _pointVisitor = new TestPairVisitor<TestEvent, TestEvent>();
        ReferenceTemporalMatcher.MatchPointToPoint<TestEvent, TestEvent, TPolicy>(
            _anchors ?? Array.Empty<TestEvent>(),
            _candidates ?? Array.Empty<TestEvent>(),
            _pointVisitor);
        return this;
    }

    public MatchingTestBuilder MatchPointToIntervalIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _pointToIntervalVisitor = new TestPairVisitor<TestEvent, TestInterval>();
        ReferenceTemporalMatcher.MatchPointToInterval<TestEvent, TestInterval, TPolicy>(
            _anchors ?? Array.Empty<TestEvent>(),
            _candidateIntervals ?? Array.Empty<TestInterval>(),
            _pointToIntervalVisitor);
        return this;
    }

    public MatchingTestBuilder MatchIntervalToPointIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _intervalToPointVisitor = new TestPairVisitor<TestInterval, TestEvent>();
        ReferenceTemporalMatcher.MatchIntervalToPoint<TestInterval, TestEvent, TPolicy>(
            _anchorIntervals ?? Array.Empty<TestInterval>(),
            _candidates ?? Array.Empty<TestEvent>(),
            _intervalToPointVisitor);
        return this;
    }

    public MatchingTestBuilder MatchIntervalToIntervalIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _intervalVisitor = new TestPairVisitor<TestInterval, TestInterval>();
        ReferenceTemporalMatcher.MatchIntervalToInterval<TestInterval, TestInterval, TPolicy>(
            _anchorIntervals ?? Array.Empty<TestInterval>(),
            _candidateIntervals ?? Array.Empty<TestInterval>(),
            _intervalVisitor);
        return this;
    }

    public MatchingTestBuilder MatchPointToPointGroupedIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _pointGroupVisitor = new TestGroupVisitor<TestEvent, TestEvent>();
        ReferenceTemporalMatcher.MatchPointToPointGrouped<TestEvent, TestEvent, TPolicy>(
            _anchors ?? Array.Empty<TestEvent>(),
            _candidates ?? Array.Empty<TestEvent>(),
            _pointGroupVisitor);
        return this;
    }

    public MatchingTestBuilder MatchPointToIntervalGroupedIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _pointToIntervalGroupVisitor = new TestGroupVisitor<TestEvent, TestInterval>();
        ReferenceTemporalMatcher.MatchPointToIntervalGrouped<TestEvent, TestInterval, TPolicy>(
            _anchors ?? Array.Empty<TestEvent>(),
            _candidateIntervals ?? Array.Empty<TestInterval>(),
            _pointToIntervalGroupVisitor);
        return this;
    }

    public MatchingTestBuilder MatchIntervalToPointGroupedIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _intervalToPointGroupVisitor = new TestGroupVisitor<TestInterval, TestEvent>();
        ReferenceTemporalMatcher.MatchIntervalToPointGrouped<TestInterval, TestEvent, TPolicy>(
            _anchorIntervals ?? Array.Empty<TestInterval>(),
            _candidates ?? Array.Empty<TestEvent>(),
            _intervalToPointGroupVisitor);
        return this;
    }

    public MatchingTestBuilder MatchIntervalToIntervalGroupedIsCalled<TPolicy>() where TPolicy : IMatchPolicy
    {
        _intervalGroupVisitor = new TestGroupVisitor<TestInterval, TestInterval>();
        ReferenceTemporalMatcher.MatchIntervalToIntervalGrouped<TestInterval, TestInterval, TPolicy>(
            _anchorIntervals ?? Array.Empty<TestInterval>(),
            _candidateIntervals ?? Array.Empty<TestInterval>(),
            _intervalGroupVisitor);
        return this;
    }

    public MatchingTestBuilder Then => this;

    public MatchingTestBuilder PairsAreFound(MatchType matchType, params (int anchorOffset, int candidateOffset)[] expectedPairs)
    {
        if (_pointVisitor != null)
        {
            Assert.That(_pointVisitor.Matches.Count, Is.EqualTo(expectedPairs.Length));
            for (int i = 0; i < expectedPairs.Length; i++)
            {
                var (anchorOffset, candidateOffset) = expectedPairs[i];
                Assert.That(_pointVisitor.Matches[i].Anchor.Name, Is.EqualTo($"Event_{anchorOffset}s"));
                Assert.That(_pointVisitor.Matches[i].Candidate.Name, Is.EqualTo($"Event_{candidateOffset}s"));
                Assert.That(_pointVisitor.Matches[i].MatchType, Is.EqualTo(matchType));
            }
        }
        return this;
    }

    public MatchingTestBuilder IntervalPairsAreFound(params (int anchorStart, int anchorEnd, int candidateStart, int candidateEnd)[] expectedPairs)
    {
        if (_intervalVisitor != null)
        {
            Assert.That(_intervalVisitor.Matches.Count, Is.EqualTo(expectedPairs.Length));
            for (int i = 0; i < expectedPairs.Length; i++)
            {
                var (anchorStart, anchorEnd, candidateStart, candidateEnd) = expectedPairs[i];
                Assert.That(_intervalVisitor.Matches[i].Anchor.Name, Is.EqualTo($"Interval_{anchorStart}s_to_{anchorEnd}s"));
                Assert.That(_intervalVisitor.Matches[i].Candidate.Name, Is.EqualTo($"Interval_{candidateStart}s_to_{candidateEnd}s"));
                Assert.That(_intervalVisitor.Matches[i].MatchType, Is.EqualTo(MatchType.Interval));
            }
        }
        return this;
    }

    public MatchingTestBuilder GroupsAreFound(params (int anchorOffset, int matchCount)[] expectedGroups)
    {
        if (_pointGroupVisitor != null)
        {
            Assert.That(_pointGroupVisitor.Groups.Count, Is.EqualTo(expectedGroups.Length));
            for (int i = 0; i < expectedGroups.Length; i++)
            {
                var (anchorOffset, matchCount) = expectedGroups[i];
                Assert.That(_pointGroupVisitor.Groups[i].Anchor.Name, Is.EqualTo($"Event_{anchorOffset}s"));
                Assert.That(_pointGroupVisitor.Groups[i].Count, Is.EqualTo(matchCount));
            }
        }
        return this;
    }

    public MatchingTestBuilder UnmatchedAnchors(params int[] offsetsInSeconds)
    {
        if (_pointVisitor != null)
        {
            Assert.That(_pointVisitor.Misses.Count, Is.EqualTo(offsetsInSeconds.Length));
            for (int i = 0; i < offsetsInSeconds.Length; i++)
            {
                Assert.That(_pointVisitor.Misses[i].Name, Is.EqualTo($"Event_{offsetsInSeconds[i]}s"));
            }
        }
        else if (_pointGroupVisitor != null)
        {
            Assert.That(_pointGroupVisitor.Misses.Count, Is.EqualTo(offsetsInSeconds.Length));
            for (int i = 0; i < offsetsInSeconds.Length; i++)
            {
                Assert.That(_pointGroupVisitor.Misses[i].Name, Is.EqualTo($"Event_{offsetsInSeconds[i]}s"));
            }
        }
        return this;
    }

    public MatchingTestBuilder TotalMatchCount(int count)
    {
        if (_pointVisitor != null)
            Assert.That(_pointVisitor.Matches.Count, Is.EqualTo(count));
        else if (_intervalVisitor != null)
            Assert.That(_intervalVisitor.Matches.Count, Is.EqualTo(count));
        else if (_pointGroupVisitor != null)
            Assert.That(_pointGroupVisitor.Groups.Count, Is.EqualTo(count));
        return this;
    }

    public MatchingTestBuilder TotalMissCount(int count)
    {
        if (_pointVisitor != null)
            Assert.That(_pointVisitor.Misses.Count, Is.EqualTo(count));
        else if (_intervalVisitor != null)
            Assert.That(_intervalVisitor.Misses.Count, Is.EqualTo(count));
        else if (_pointGroupVisitor != null)
            Assert.That(_pointGroupVisitor.Misses.Count, Is.EqualTo(count));
        return this;
    }

    public MatchingTestBuilder TotalMatchCount(IResolveConstraint constraint)
    {
        if (_pointVisitor != null)
            Assert.That(_pointVisitor.Matches.Count, constraint);
        else if (_intervalVisitor != null)
            Assert.That(_intervalVisitor.Matches.Count, constraint);
        else if (_pointToIntervalVisitor != null)
            Assert.That(_pointToIntervalVisitor.Matches.Count, constraint);
        else if (_pointGroupVisitor != null)
            Assert.That(_pointGroupVisitor.Groups.Count, constraint);
        return this;
    }

    public MatchingTestBuilder AllMatchesHaveType(MatchType expectedType)
    {
        if (_intervalVisitor != null)
        {
            foreach (var match in _intervalVisitor.Matches)
            {
                Assert.That(match.MatchType, Is.EqualTo(expectedType));
            }
        }
        return this;
    }

    public MatchingTestBuilder MatchHasRelation(TemporalRelation expectedRelation)
    {
        if (_intervalVisitor != null && _intervalVisitor.Matches.Count > 0)
        {
            Assert.That(_intervalVisitor.Matches[0].Relation, Is.EqualTo(expectedRelation));
        }
        return this;
    }

    private class TestPairVisitor<TAnchor, TCandidate> : IPairMatchVisitor<TAnchor, TCandidate>
    {
        public List<MatchPair<TAnchor, TCandidate>> Matches { get; } = new();
        public List<TAnchor> Misses { get; } = new();

        public void OnMatch(in MatchPair<TAnchor, TCandidate> pair) => Matches.Add(pair);
        public void OnMiss(TAnchor anchor) => Misses.Add(anchor);
    }

    private class TestGroupVisitor<TAnchor, TCandidate> : IGroupMatchVisitor<TAnchor, TCandidate>
    {
        public List<MatchGroup<TAnchor, TCandidate>> Groups { get; } = new();
        public List<TAnchor> Misses { get; } = new();

        public void OnMatch(in MatchGroup<TAnchor, TCandidate> group) => Groups.Add(group);
        public void OnMiss(TAnchor anchor) => Misses.Add(anchor);
    }
}

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

[TestFixture]
public class IntervalToIntervalMatchingTests : MatchingTestBase
{
    [Test]
    public void Should_Match_All_Allen_Relations()
    {
        var pairs = TestDataGenerator.CreateAllAllenRelations();
        var visitor = new TestPairVisitor<TestInterval, TestInterval>();

        foreach (var (anchor, candidate) in pairs)
        {
            visitor.Clear();
            var anchors = new[] { anchor };
            var candidates = new[] { candidate };

            ReferenceTemporalMatcher.MatchIntervalToInterval<TestInterval, TestInterval, ExactMatchPolicy>(
                anchors, candidates, visitor);

            Assert.That(visitor.Matches.Count, Is.EqualTo(1),
                $"Expected match for {anchor.Name} and {candidate.Name}");
            Assert.That(visitor.Matches[0].MatchType, Is.EqualTo(MatchType.Interval));
        }
    }

    [Test]
    public void FilteredRelations_Should_Only_Match_Allowed_Relations()
    {
        var allPairs = TestDataGenerator.CreateAllAllenRelations();
        var visitor = new TestPairVisitor<TestInterval, TestInterval>();

        foreach (var (anchor, candidate) in allPairs)
        {
            visitor.Clear();
            var anchors = new[] { anchor };
            var candidates = new[] { candidate };

            ReferenceTemporalMatcher.MatchIntervalToInterval<TestInterval, TestInterval, FilteredRelationsPolicy>(
                anchors, candidates, visitor);

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
    public void EqualRelation_Should_Match_Identical_Intervals()
    {
        Given
            .AnchorIntervals((10, 30))
            .CandidateIntervals((10, 30))
        .When
            .MatchIntervalToIntervalIsCalled<ExactMatchPolicy>()
        .Then
            .IntervalPairsAreFound((10, 30, 10, 30));
    }

    #region Test Helpers

    private class TestPairVisitor<TAnchor, TCandidate> : IPairMatchVisitor<TAnchor, TCandidate>
    {
        public List<MatchPair<TAnchor, TCandidate>> Matches { get; } = new();
        public List<TAnchor> Misses { get; } = new();

        public void OnMatch(in MatchPair<TAnchor, TCandidate> pair) => Matches.Add(pair);
        public void OnMiss(TAnchor anchor) => Misses.Add(anchor);

        public void Clear()
        {
            Matches.Clear();
            Misses.Clear();
        }
    }

    #endregion
}

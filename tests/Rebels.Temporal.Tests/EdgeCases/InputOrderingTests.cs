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

using Rebels.Temporal.Tests.Reference;
using Rebels.Temporal.Tests.TestData;

namespace Rebels.Temporal.Tests.EdgeCases;

/// <summary>
/// Tests for InputOrdering validation and optimized algorithms.
/// Per INV-10, when ordering is declared, the data must be sorted.
/// </summary>
[TestFixture]
public class InputOrderingTests : MatchingTestBase
{
    #region Sorted Candidates - Binary Search Algorithm

    [Test]
    public void SortedCandidates_Should_Match_Correctly_With_Sorted_Data()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void SortedCandidates_Should_Throw_When_Candidates_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Candidates are not sorted: 20, 10, 0
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(0, 10, 20);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(20, 10, 0);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.SortedCandidates, ref visitor);
        });
    }

    #endregion

    #region Both Sorted - Dual Pointer Algorithm

    [Test]
    public void BothSorted_Should_Match_Correctly_With_Sorted_Data()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSorted)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void BothSorted_Should_Throw_When_Anchors_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Anchors are not sorted: 20, 10, 0
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(20, 10, 0);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(0, 10, 20);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.BothSorted, ref visitor);
        });
    }

    [Test]
    public void BothSorted_Should_Throw_When_Candidates_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            // Candidates are not sorted: 20, 10, 0
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(0, 10, 20);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(20, 10, 0);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.BothSorted, ref visitor);
        });
    }

    [Test]
    public void BothSorted_Should_Throw_When_Both_Not_Sorted()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ReadOnlySpan<TestEvent> anchors = TestDataGenerator.CreatePoints(20, 10, 0);
            ReadOnlySpan<TestEvent> candidates = TestDataGenerator.CreatePoints(30, 20, 10);

            var buffer = new MatchPair<TestEvent, TestEvent>[100];
            var visitor = new BufferVisitor<TestEvent, TestEvent>(buffer);
            MatchTemporal.Points.With.Points(anchors, candidates, TestPolicies.BothSorted, ref visitor);
        });
    }

    #endregion

    #region No Ordering - Unsorted Data Accepted

    [Test]
    public void NoOrdering_Should_Accept_Unsorted_Anchors()
    {
        Given
            .AnchorOffsets(20, 10, 0)
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void NoOrdering_Should_Accept_Unsorted_Candidates()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(20, 10, 0)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(3);
    }

    [Test]
    public void NoOrdering_Should_Accept_Both_Unsorted()
    {
        Given
            .AnchorOffsets(20, 10, 0)
            .CandidateOffsets(30, 20, 10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.ExactMatch)
        .Then
            .TotalMatchCount(2); // 20 matches 20, 10 matches 10
    }

    #endregion

    #region Edge Cases for Ordering

    [Test]
    public void SortedCandidates_Should_Accept_Equal_Consecutive_Values()
    {
        // Equal consecutive values are allowed (non-decreasing order)
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(0, 0, 10, 10, 20, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(6); // Each anchor matches two candidates
    }

    [Test]
    public void BothSorted_Should_Accept_Equal_Consecutive_Values()
    {
        Given
            .AnchorOffsets(0, 0, 10, 10)
            .CandidateOffsets(0, 0, 10, 10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSorted)
        .Then
            .TotalMatchCount(8); // 2x2 + 2x2
    }

    [Test]
    public void SortedCandidates_Should_Work_With_Empty_Collections()
    {
        Given
            .AnchorOffsets()
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void BothSorted_Should_Work_With_Empty_Collections()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets()
        .When
            .MatchPointToPointIsCalled(TestPolicies.BothSorted)
        .Then
            .TotalMatchCount(0);
    }

    [Test]
    public void SortedCandidates_Should_Work_With_Single_Element()
    {
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(10)
        .When
            .MatchPointToPointIsCalled(TestPolicies.SortedCandidates)
        .Then
            .TotalMatchCount(1);
    }

    #endregion
}

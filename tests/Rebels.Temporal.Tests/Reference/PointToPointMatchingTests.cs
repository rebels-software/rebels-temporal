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
public class PointToPointMatchingTests : MatchingTestBase
{
    [Test]
    public void ExactPolicy_Should_Match_Exact_Timestamps()
    {
        Given
            .AnchorOffsets(0, 10, 20, 30)
            .CandidateOffsets(10, 20, 40, 50)
        .When
            .MatchPointToPointIsCalled<ExactMatchPolicy>()
        .Then
            .PairsAreFound(MatchType.PointExact, (10, 10), (20, 20))
            .UnmatchedAnchors(0, 30);
    }

    [Test]
    public void SymmetricTolerance_Should_Match_Within_Window()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(2, 12, 25)
        .When
            .MatchPointToPointIsCalled<SymmetricTolerancePolicy>()
        .Then
            .PairsAreFound(MatchType.PointInInterval, (0, 2), (10, 12), (20, 25))
            .TotalMissCount(0);
    }

    [Test]
    public void BothSidesTolerance_Should_Use_Allen_Algebra()
    {
        Given
            .AnchorOffsets(10)
            .CandidateOffsets(12)
        .When
            .MatchPointToPointIsCalled<BothSidesTolerancePolicy>()
        .Then
            .TotalMatchCount(1)
            .TotalMissCount(0);
    }

    [Test]
    public void EmptyAnchors_Should_Return_No_Results()
    {
        Given
            .AnchorOffsets()
            .CandidateOffsets(0, 10, 20)
        .When
            .MatchPointToPointIsCalled<ExactMatchPolicy>()
        .Then
            .TotalMatchCount(0)
            .TotalMissCount(0);
    }

    [Test]
    public void EmptyCandidates_Should_Return_All_Misses()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets()
        .When
            .MatchPointToPointIsCalled<ExactMatchPolicy>()
        .Then
            .TotalMatchCount(0)
            .UnmatchedAnchors(0, 10, 20);
    }

    [Test]
    public void AllMatch_Should_Return_All_Matches()
    {
        Given
            .AnchorOffsets(0, 0, 0, 0, 0)
            .CandidateOffsets(0, 0, 0, 0, 0)
        .When
            .MatchPointToPointIsCalled<ExactMatchPolicy>()
        .Then
            .TotalMatchCount(25)
            .TotalMissCount(0);
    }

    [Test]
    public void NoMatch_Should_Return_All_Misses()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets(5, 15, 25)
        .When
            .MatchPointToPointIsCalled<ExactMatchPolicy>()
        .Then
            .TotalMatchCount(0)
            .TotalMissCount(3);
    }

    [Test]
    public void ToleranceBoundary_Should_Match_At_Exact_Boundary()
    {
        Given
            .AnchorOffsets(0)
            .CandidateOffsets(-6, -5, 0, 5, 6)
        .When
            .MatchPointToPointIsCalled<SymmetricTolerancePolicy>()
        .Then
            .TotalMatchCount(3)
            .TotalMissCount(0);
    }
}

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
public class PointToPointGroupedMatchingTests : MatchingTestBase
{
    [Test]
    public void ExactPolicy_Should_Group_Matches_By_Anchor()
    {
        Given
            .AnchorOffsets(0, 10)
            .CandidateOffsets(10, 10, 10)
        .When
            .MatchPointToPointGroupedIsCalled<ExactMatchPolicy>()
        .Then
            .GroupsAreFound((10, 3))
            .UnmatchedAnchors(0);
    }

    [Test]
    public void OneToMany_Should_Create_Single_Group()
    {
        Given
            .AnchorOffsets(0)
            .CandidateOffsets(0, 0, 0, 0, 0)
        .When
            .MatchPointToPointGroupedIsCalled<ExactMatchPolicy>()
        .Then
            .GroupsAreFound((0, 5))
            .TotalMissCount(0);
    }

    [Test]
    public void EmptyCandidates_Should_Return_All_Misses()
    {
        Given
            .AnchorOffsets(0, 10, 20)
            .CandidateOffsets()
        .When
            .MatchPointToPointGroupedIsCalled<ExactMatchPolicy>()
        .Then
            .TotalMatchCount(0)
            .UnmatchedAnchors(0, 10, 20);
    }
}

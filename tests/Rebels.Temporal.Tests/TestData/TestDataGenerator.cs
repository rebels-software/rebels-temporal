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

namespace Rebels.Temporal.Tests.TestData;

/// <summary>
/// Generates test data for temporal matching scenarios.
/// </summary>
public static class TestDataGenerator
{
    private static readonly DateTimeOffset BaseTime = new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero);

    #region Point Generation

    /// <summary>
    /// Creates an array of test events with exact timestamps.
    /// </summary>
    public static TestEvent[] CreatePoints(params int[] secondsOffsets)
    {
        var result = new TestEvent[secondsOffsets.Length];
        for (int i = 0; i < secondsOffsets.Length; i++)
        {
            result[i] = new TestEvent(
                BaseTime.AddSeconds(secondsOffsets[i]),
                $"Event_{secondsOffsets[i]}s");
        }
        return result;
    }

    /// <summary>
    /// Creates an array of test events at the same timestamp.
    /// </summary>
    public static TestEvent[] CreateCoincidentPoints(int count, int secondsOffset = 0)
    {
        var timestamp = BaseTime.AddSeconds(secondsOffset);
        var result = new TestEvent[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = new TestEvent(timestamp, $"Coincident_{i}");
        }
        return result;
    }

    /// <summary>
    /// Creates an array of test events with regular intervals.
    /// </summary>
    public static TestEvent[] CreateRegularPoints(int count, int intervalSeconds, int startOffset = 0)
    {
        var result = new TestEvent[count];
        for (int i = 0; i < count; i++)
        {
            var offset = startOffset + (i * intervalSeconds);
            result[i] = new TestEvent(
                BaseTime.AddSeconds(offset),
                $"Regular_{i}_{offset}s");
        }
        return result;
    }

    #endregion

    #region Interval Generation

    /// <summary>
    /// Creates an array of test intervals with specified start/end offsets.
    /// </summary>
    public static TestInterval[] CreateIntervals(params (int start, int end)[] ranges)
    {
        var result = new TestInterval[ranges.Length];
        for (int i = 0; i < ranges.Length; i++)
        {
            result[i] = new TestInterval(
                BaseTime.AddSeconds(ranges[i].start),
                BaseTime.AddSeconds(ranges[i].end),
                $"Interval_{ranges[i].start}s_to_{ranges[i].end}s");
        }
        return result;
    }

    /// <summary>
    /// Creates two intervals demonstrating a specific Allen relation.
    /// </summary>
    public static (TestInterval anchor, TestInterval candidate) CreateAllenRelation(TemporalRelation relation)
    {
        return relation switch
        {
            // anchor: [10, 20], candidate: [30, 40]
            TemporalRelation.Before => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(20), "Anchor_Before"),
                new TestInterval(BaseTime.AddSeconds(30), BaseTime.AddSeconds(40), "Candidate_After")),

            // anchor: [30, 40], candidate: [10, 20]
            TemporalRelation.After => (
                new TestInterval(BaseTime.AddSeconds(30), BaseTime.AddSeconds(40), "Anchor_After"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(20), "Candidate_Before")),

            // anchor: [10, 20], candidate: [20, 30]
            TemporalRelation.Meets => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(20), "Anchor_Meets"),
                new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(30), "Candidate_MetBy")),

            // anchor: [20, 30], candidate: [10, 20]
            TemporalRelation.MetBy => (
                new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(30), "Anchor_MetBy"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(20), "Candidate_Meets")),

            // anchor: [10, 25], candidate: [20, 30]
            TemporalRelation.Overlaps => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(25), "Anchor_Overlaps"),
                new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(30), "Candidate_OverlappedBy")),

            // anchor: [20, 30], candidate: [10, 25]
            TemporalRelation.OverlappedBy => (
                new TestInterval(BaseTime.AddSeconds(20), BaseTime.AddSeconds(30), "Anchor_OverlappedBy"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(25), "Candidate_Overlaps")),

            // anchor: [15, 25], candidate: [10, 30]
            TemporalRelation.During => (
                new TestInterval(BaseTime.AddSeconds(15), BaseTime.AddSeconds(25), "Anchor_During"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Candidate_Contains")),

            // anchor: [10, 30], candidate: [15, 25]
            TemporalRelation.Contains => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Anchor_Contains"),
                new TestInterval(BaseTime.AddSeconds(15), BaseTime.AddSeconds(25), "Candidate_During")),

            // anchor: [10, 25], candidate: [10, 30]
            TemporalRelation.Starts => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(25), "Anchor_Starts"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Candidate_StartedBy")),

            // anchor: [10, 30], candidate: [10, 25]
            TemporalRelation.StartedBy => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Anchor_StartedBy"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(25), "Candidate_Starts")),

            // anchor: [15, 30], candidate: [10, 30]
            TemporalRelation.Finishes => (
                new TestInterval(BaseTime.AddSeconds(15), BaseTime.AddSeconds(30), "Anchor_Finishes"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Candidate_FinishedBy")),

            // anchor: [10, 30], candidate: [15, 30]
            TemporalRelation.FinishedBy => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Anchor_FinishedBy"),
                new TestInterval(BaseTime.AddSeconds(15), BaseTime.AddSeconds(30), "Candidate_Finishes")),

            // anchor: [10, 30], candidate: [10, 30]
            TemporalRelation.Equal => (
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Anchor_Equal"),
                new TestInterval(BaseTime.AddSeconds(10), BaseTime.AddSeconds(30), "Candidate_Equal")),

            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "Unknown Allen relation")
        };
    }

    /// <summary>
    /// Creates a collection of interval pairs covering all 13 Allen relations.
    /// </summary>
    public static (TestInterval anchor, TestInterval candidate)[] CreateAllAllenRelations()
    {
        var relations = Enum.GetValues<TemporalRelation>();
        var result = new (TestInterval, TestInterval)[relations.Length];

        for (int i = 0; i < relations.Length; i++)
        {
            result[i] = CreateAllenRelation(relations[i]);
        }

        return result;
    }

    #endregion

    #region Edge Cases

    /// <summary>
    /// Creates empty array of test events.
    /// </summary>
    public static TestEvent[] CreateEmptyPoints() => Array.Empty<TestEvent>();

    /// <summary>
    /// Creates empty array of test intervals.
    /// </summary>
    public static TestInterval[] CreateEmptyIntervals() => Array.Empty<TestInterval>();

    /// <summary>
    /// Creates a single test event.
    /// </summary>
    public static TestEvent[] CreateSinglePoint(int secondsOffset = 0)
    {
        return new[] { new TestEvent(BaseTime.AddSeconds(secondsOffset), $"Single_{secondsOffset}s") };
    }

    /// <summary>
    /// Creates a single test interval.
    /// </summary>
    public static TestInterval[] CreateSingleInterval(int startOffset = 0, int endOffset = 10)
    {
        return new[]
        {
            new TestInterval(
                BaseTime.AddSeconds(startOffset),
                BaseTime.AddSeconds(endOffset),
                $"Single_{startOffset}s_to_{endOffset}s")
        };
    }

    #endregion

    #region Tolerance Boundary Cases

    /// <summary>
    /// Creates points at the exact boundary of a tolerance window.
    /// </summary>
    public static (TestEvent anchor, TestEvent[] candidates) CreateToleranceBoundaryPoints(
        int toleranceSeconds)
    {
        var anchor = new TestEvent(BaseTime, "Anchor");
        var candidates = new[]
        {
            new TestEvent(BaseTime.AddSeconds(-toleranceSeconds - 1), "BeforeTolerance"),
            new TestEvent(BaseTime.AddSeconds(-toleranceSeconds), "AtLowerBoundary"),
            new TestEvent(BaseTime.AddSeconds(0), "Exact"),
            new TestEvent(BaseTime.AddSeconds(toleranceSeconds), "AtUpperBoundary"),
            new TestEvent(BaseTime.AddSeconds(toleranceSeconds + 1), "AfterTolerance")
        };
        return (anchor, candidates);
    }

    /// <summary>
    /// Creates intervals at tolerance boundaries for point-to-interval matching.
    /// </summary>
    public static (TestEvent anchor, TestInterval[] candidates) CreatePointToIntervalBoundaries(
        int toleranceSeconds)
    {
        var anchor = new TestEvent(BaseTime, "Anchor");
        var candidates = new[]
        {
            // Interval completely before tolerance window
            new TestInterval(
                BaseTime.AddSeconds(-toleranceSeconds - 10),
                BaseTime.AddSeconds(-toleranceSeconds - 5),
                "BeforeTolerance"),

            // Interval ends at lower boundary
            new TestInterval(
                BaseTime.AddSeconds(-toleranceSeconds - 5),
                BaseTime.AddSeconds(-toleranceSeconds),
                "EndsAtLowerBoundary"),

            // Interval starts at lower boundary
            new TestInterval(
                BaseTime.AddSeconds(-toleranceSeconds),
                BaseTime.AddSeconds(-toleranceSeconds + 5),
                "StartsAtLowerBoundary"),

            // Interval contains exact point
            new TestInterval(
                BaseTime.AddSeconds(-5),
                BaseTime.AddSeconds(5),
                "ContainsExact"),

            // Interval starts at upper boundary
            new TestInterval(
                BaseTime.AddSeconds(toleranceSeconds - 5),
                BaseTime.AddSeconds(toleranceSeconds),
                "EndsAtUpperBoundary"),

            // Interval ends at upper boundary
            new TestInterval(
                BaseTime.AddSeconds(toleranceSeconds),
                BaseTime.AddSeconds(toleranceSeconds + 5),
                "StartsAtUpperBoundary"),

            // Interval completely after tolerance window
            new TestInterval(
                BaseTime.AddSeconds(toleranceSeconds + 5),
                BaseTime.AddSeconds(toleranceSeconds + 10),
                "AfterTolerance")
        };
        return (anchor, candidates);
    }

    #endregion

    #region Sorting Scenarios

    /// <summary>
    /// Creates an unsorted array of test events.
    /// </summary>
    public static TestEvent[] CreateUnsortedPoints()
    {
        return CreatePoints(50, 10, 30, 0, 20, 40);
    }

    /// <summary>
    /// Creates a sorted array of test events.
    /// </summary>
    public static TestEvent[] CreateSortedPoints()
    {
        return CreatePoints(0, 10, 20, 30, 40, 50);
    }

    /// <summary>
    /// Creates an unsorted array of test intervals.
    /// </summary>
    public static TestInterval[] CreateUnsortedIntervals()
    {
        return CreateIntervals(
            (30, 40),
            (0, 10),
            (50, 60),
            (10, 20),
            (40, 50),
            (20, 30));
    }

    /// <summary>
    /// Creates a sorted array of test intervals (by start time).
    /// </summary>
    public static TestInterval[] CreateSortedIntervals()
    {
        return CreateIntervals(
            (0, 10),
            (10, 20),
            (20, 30),
            (30, 40),
            (40, 50),
            (50, 60));
    }

    #endregion

    #region Complex Scenarios

    /// <summary>
    /// Creates a scenario where all candidates match all anchors.
    /// </summary>
    public static (TestEvent[] anchors, TestEvent[] candidates) CreateAllMatch(int count)
    {
        var anchors = CreateCoincidentPoints(count, 0);
        var candidates = CreateCoincidentPoints(count, 0);
        return (anchors, candidates);
    }

    /// <summary>
    /// Creates a scenario where no candidates match any anchors.
    /// </summary>
    public static (TestEvent[] anchors, TestEvent[] candidates) CreateNoMatch()
    {
        var anchors = CreatePoints(0, 10, 20);
        var candidates = CreatePoints(5, 15, 25);
        return (anchors, candidates);
    }

    /// <summary>
    /// Creates a scenario with one-to-many matching.
    /// </summary>
    public static (TestEvent[] anchors, TestEvent[] candidates) CreateOneToMany()
    {
        var anchors = new[] { new TestEvent(BaseTime, "Anchor") };
        var candidates = CreateCoincidentPoints(5, 0);
        return (anchors, candidates);
    }

    /// <summary>
    /// Creates a scenario with many-to-one matching.
    /// </summary>
    public static (TestEvent[] anchors, TestEvent[] candidates) CreateManyToOne()
    {
        var anchors = CreateCoincidentPoints(5, 0);
        var candidates = new[] { new TestEvent(BaseTime, "Candidate") };
        return (anchors, candidates);
    }

    /// <summary>
    /// Creates a dense clustering of events within a small time window.
    /// </summary>
    public static TestEvent[] CreateDenseCluster(int count, int windowSeconds)
    {
        var result = new TestEvent[count];
        var random = new Random(42); // Deterministic seed

        for (int i = 0; i < count; i++)
        {
            var offset = random.Next(0, windowSeconds);
            result[i] = new TestEvent(
                BaseTime.AddSeconds(offset),
                $"Cluster_{i}_{offset}s");
        }

        return result;
    }

    #endregion
}

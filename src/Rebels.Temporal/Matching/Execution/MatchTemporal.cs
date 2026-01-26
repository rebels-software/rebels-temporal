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

namespace Rebels.Temporal;

/// <summary>
/// Provides high-performance temporal matching algorithms for correlating
/// anchors and candidates based on their temporal characteristics.
/// </summary>
public static class MatchTemporal
{
    /// <summary>
    /// Entry point for matching point anchors.
    /// </summary>
    public static PointAnchors Points => default;

    /// <summary>
    /// Entry point for matching interval anchors.
    /// </summary>
    public static IntervalAnchors Intervals => default;

    /// <summary>
    /// Fluent API for matching point anchors.
    /// </summary>
    public readonly struct PointAnchors
    {
        /// <summary>
        /// Specify what to match point anchors with.
        /// </summary>
        public PointAnchorsWith With => default;
    }

    /// <summary>
    /// Fluent API for specifying point anchor candidates.
    /// </summary>
    public readonly struct PointAnchorsWith
    {
        /// <summary>
        /// Matches point anchors with point candidates.
        /// </summary>
        /// <typeparam name="TAnchor">Type of anchor events.</typeparam>
        /// <typeparam name="TCandidate">Type of candidate events.</typeparam>
        /// <typeparam name="TVisitor">Type of visitor.</typeparam>
        /// <param name="anchors">Collection of anchor events.</param>
        /// <param name="candidates">Collection of candidate events.</param>
        /// <param name="policy">Matching policy.</param>
        /// <param name="visitor">Visitor to receive match notifications.</param>
        public void Points<TAnchor, TCandidate, TVisitor>(
            ReadOnlySpan<TAnchor> anchors,
            ReadOnlySpan<TCandidate> candidates,
            MatchPolicy policy,
            ref TVisitor visitor)
            where TAnchor : ITemporalPoint
            where TCandidate : ITemporalPoint
            where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
        {
            // Validate ordering if specified
            if (policy.InputOrdering == InputOrdering.Both)
            {
                ValidatePointOrdering(anchors, nameof(anchors));
                ValidatePointOrdering(candidates, nameof(candidates));
            }
            else if (policy.InputOrdering == InputOrdering.Candidates)
            {
                ValidatePointOrdering(candidates, nameof(candidates));
            }

            // Select algorithm based on ordering
            switch (policy.InputOrdering)
            {
                case InputOrdering.Both:
                    MatchPointToPointSorted(anchors, candidates, policy, ref visitor);
                    break;
                case InputOrdering.Candidates:
                    MatchPointToPointCandidatesSorted(anchors, candidates, policy, ref visitor);
                    break;
                default:
                    MatchPointToPointUnsorted(anchors, candidates, policy, ref visitor);
                    break;
            }
        }

        /// <summary>
        /// Matches point anchors with interval candidates.
        /// </summary>
        public void Intervals<TAnchor, TCandidate, TVisitor>(
            ReadOnlySpan<TAnchor> anchors,
            ReadOnlySpan<TCandidate> candidates,
            MatchPolicy policy,
            ref TVisitor visitor)
            where TAnchor : ITemporalPoint
            where TCandidate : ITemporalInterval
            where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
        {
            // Validate interval correctness
            ValidateIntervals(candidates, nameof(candidates));

            MatchPointToInterval(anchors, candidates, policy, ref visitor);
        }
    }

    /// <summary>
    /// Fluent API for matching interval anchors.
    /// </summary>
    public readonly struct IntervalAnchors
    {
        /// <summary>
        /// Specify what to match interval anchors with.
        /// </summary>
        public IntervalAnchorsWith With => default;
    }

    /// <summary>
    /// Fluent API for specifying interval anchor candidates.
    /// </summary>
    public readonly struct IntervalAnchorsWith
    {
        /// <summary>
        /// Matches interval anchors with point candidates.
        /// </summary>
        public void Points<TAnchor, TCandidate, TVisitor>(
            ReadOnlySpan<TAnchor> anchors,
            ReadOnlySpan<TCandidate> candidates,
            MatchPolicy policy,
            ref TVisitor visitor)
            where TAnchor : ITemporalInterval
            where TCandidate : ITemporalPoint
            where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
        {
            // Validate interval correctness
            ValidateIntervals(anchors, nameof(anchors));

            MatchIntervalToPoint(anchors, candidates, policy, ref visitor);
        }

        /// <summary>
        /// Matches interval anchors with interval candidates.
        /// </summary>
        public void Intervals<TAnchor, TCandidate, TVisitor>(
            ReadOnlySpan<TAnchor> anchors,
            ReadOnlySpan<TCandidate> candidates,
            MatchPolicy policy,
            ref TVisitor visitor)
            where TAnchor : ITemporalInterval
            where TCandidate : ITemporalInterval
            where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
        {
            // Validate interval correctness
            ValidateIntervals(anchors, nameof(anchors));
            ValidateIntervals(candidates, nameof(candidates));

            MatchIntervalToInterval(anchors, candidates, policy, ref visitor);
        }
    }

    #region Point-to-Point Matching

    private static void MatchPointToPointUnsorted<TAnchor, TCandidate, TVisitor>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        MatchPolicy policy,
        ref TVisitor visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
    {
        var anchorTolerance = policy.AnchorTolerance;

        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var windowStart = anchorTime - anchorTolerance.Before;
            var windowEnd = anchorTime + anchorTolerance.After;
            bool anchorMatched = false;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                if (candidateTime >= windowStart && candidateTime <= windowEnd)
                {
                    visitor.OnMatch(in anchor, in candidate, i, j, MatchType.PointExact, null);
                    anchorMatched = true;
                }
            }

            if (!anchorMatched)
            {
                visitor.OnUnmatchedAnchor(in anchor, i);
            }
        }
    }

    private static void MatchPointToPointSorted<TAnchor, TCandidate, TVisitor>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        MatchPolicy policy,
        ref TVisitor visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
    {
        var anchorTolerance = policy.AnchorTolerance;
        int candidateIndex = 0;

        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var windowStart = anchorTime - anchorTolerance.Before;
            var windowEnd = anchorTime + anchorTolerance.After;
            bool anchorMatched = false;

            // Advance candidateIndex to window start
            while (candidateIndex < candidates.Length &&
                   candidates[candidateIndex].At < windowStart)
            {
                candidateIndex++;
            }

            // Scan candidates in window
            int j = candidateIndex;
            while (j < candidates.Length && candidates[j].At <= windowEnd)
            {
                visitor.OnMatch(in anchor, in candidates[j], i, j, MatchType.PointExact, null);
                anchorMatched = true;
                j++;
            }

            if (!anchorMatched)
            {
                visitor.OnUnmatchedAnchor(in anchor, i);
            }
        }
    }

    private static void MatchPointToPointCandidatesSorted<TAnchor, TCandidate, TVisitor>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        MatchPolicy policy,
        ref TVisitor visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
    {
        var anchorTolerance = policy.AnchorTolerance;

        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var windowStart = anchorTime - anchorTolerance.Before;
            var windowEnd = anchorTime + anchorTolerance.After;
            bool anchorMatched = false;

            // Binary search for window start
            int startIdx = BinarySearchLowerBound(candidates, windowStart);

            // Scan forward until window end
            for (int j = startIdx; j < candidates.Length && candidates[j].At <= windowEnd; j++)
            {
                visitor.OnMatch(in anchor, in candidates[j], i, j, MatchType.PointExact, null);
                anchorMatched = true;
            }

            if (!anchorMatched)
            {
                visitor.OnUnmatchedAnchor(in anchor, i);
            }
        }
    }

    #endregion

    #region Point-to-Interval Matching

    private static void MatchPointToInterval<TAnchor, TCandidate, TVisitor>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        MatchPolicy policy,
        ref TVisitor visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
    {
        var anchorTolerance = policy.AnchorTolerance;

        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var windowStart = anchorTime - anchorTolerance.Before;
            var windowEnd = anchorTime + anchorTolerance.After;
            bool anchorMatched = false;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];

                if (windowStart <= candidate.End && windowEnd >= candidate.Start)
                {
                    visitor.OnMatch(in anchor, in candidate, i, j, MatchType.PointInInterval, null);
                    anchorMatched = true;
                }
            }

            if (!anchorMatched)
            {
                visitor.OnUnmatchedAnchor(in anchor, i);
            }
        }
    }

    #endregion

    #region Interval-to-Point Matching

    private static void MatchIntervalToPoint<TAnchor, TCandidate, TVisitor>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        MatchPolicy policy,
        ref TVisitor visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
    {
        var candidateTolerance = policy.CandidateTolerance;

        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            bool anchorMatched = false;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;
                var candidateWindowStart = candidateTime - candidateTolerance.Before;
                var candidateWindowEnd = candidateTime + candidateTolerance.After;

                if (anchor.Start <= candidateWindowEnd && anchor.End >= candidateWindowStart)
                {
                    visitor.OnMatch(in anchor, in candidate, i, j, MatchType.PointInInterval, null);
                    anchorMatched = true;
                }
            }

            if (!anchorMatched)
            {
                visitor.OnUnmatchedAnchor(in anchor, i);
            }
        }
    }

    #endregion

    #region Interval-to-Interval Matching

    private static void MatchIntervalToInterval<TAnchor, TCandidate, TVisitor>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        MatchPolicy policy,
        ref TVisitor visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TVisitor : IMatchVisitor<TAnchor, TCandidate>, allows ref struct
    {
        var allowedRelations = policy.AllowedTemporalRelations;

        if (allowedRelations == AllowedRelations.Any)
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var relation = DetermineAllenRelation(
                        anchor.Start, anchor.End,
                        candidate.Start, candidate.End);

                    visitor.OnMatch(in anchor, in candidate, i, j, MatchType.Interval, relation);
                }

                // With AllowedRelations.Any, every anchor matches every candidate, so no unmatched anchors
                // (unless candidates is empty)
                if (candidates.Length == 0)
                {
                    visitor.OnUnmatchedAnchor(in anchor, i);
                }
            }
        }
        else
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                bool anchorMatched = false;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var relation = DetermineAllenRelation(
                        anchor.Start, anchor.End,
                        candidate.Start, candidate.End);

                    if (IsRelationAllowed(relation, allowedRelations))
                    {
                        visitor.OnMatch(in anchor, in candidate, i, j, MatchType.Interval, relation);
                        anchorMatched = true;
                    }
                }

                if (!anchorMatched)
                {
                    visitor.OnUnmatchedAnchor(in anchor, i);
                }
            }
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Validates that all intervals have Start <= End.
    /// </summary>
    private static void ValidateIntervals<T>(ReadOnlySpan<T> intervals, string paramName)
        where T : ITemporalInterval
    {
        for (int i = 0; i < intervals.Length; i++)
        {
            if (intervals[i].Start > intervals[i].End)
            {
                throw new ArgumentException(
                    $"Invalid interval at index {i}: Start ({intervals[i].Start}) is after End ({intervals[i].End})",
                    paramName);
            }
        }
    }

    /// <summary>
    /// Validates that point elements are sorted in ascending order.
    /// </summary>
    private static void ValidatePointOrdering<T>(ReadOnlySpan<T> points, string paramName)
        where T : ITemporalPoint
    {
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].At < points[i - 1].At)
            {
                throw new ArgumentException(
                    $"Elements are not sorted: element at index {i} ({points[i].At}) is before element at index {i - 1} ({points[i - 1].At})",
                    paramName);
            }
        }
    }

    /// <summary>
    /// Binary search to find the first element >= target.
    /// </summary>
    private static int BinarySearchLowerBound<T>(ReadOnlySpan<T> candidates, DateTimeOffset target)
        where T : ITemporalPoint
    {
        int left = 0;
        int right = candidates.Length;

        while (left < right)
        {
            int mid = left + (right - left) / 2;

            if (candidates[mid].At < target)
                left = mid + 1;
            else
                right = mid;
        }

        return left;
    }

    /// <summary>
    /// Determines the Allen's interval algebra relation between two temporal intervals.
    /// </summary>
    private static TemporalRelation DetermineAllenRelation(
        DateTimeOffset aStart,
        DateTimeOffset aEnd,
        DateTimeOffset bStart,
        DateTimeOffset bEnd)
    {
        // First check for completely disjoint intervals (Before/After)
        if (aEnd < bStart) return TemporalRelation.Before;
        if (aStart > bEnd) return TemporalRelation.After;

        // Check for exact equality first (handles zero-duration intervals correctly)
        if (aStart == bStart && aEnd == bEnd) return TemporalRelation.Equal;

        // Check for shared start (Starts/StartedBy) - must come before Meets check
        if (aStart == bStart && aEnd < bEnd) return TemporalRelation.Starts;
        if (aStart == bStart && aEnd > bEnd) return TemporalRelation.StartedBy;

        // Check for shared end (Finishes/FinishedBy) - must come before MetBy check
        if (aEnd == bEnd && aStart > bStart) return TemporalRelation.Finishes;
        if (aEnd == bEnd && aStart < bStart) return TemporalRelation.FinishedBy;

        // Check for touching intervals (Meets/MetBy)
        if (aEnd == bStart) return TemporalRelation.Meets;
        if (aStart == bEnd) return TemporalRelation.MetBy;

        // Check for containment (During/Contains)
        if (aStart > bStart && aEnd < bEnd) return TemporalRelation.During;
        if (aStart < bStart && aEnd > bEnd) return TemporalRelation.Contains;

        // Remaining cases are overlaps
        if (aStart < bStart) return TemporalRelation.Overlaps;
        return TemporalRelation.OverlappedBy;
    }

    /// <summary>
    /// Checks if a specific relation is allowed by the policy.
    /// </summary>
    private static bool IsRelationAllowed(TemporalRelation relation, AllowedRelations allowed)
    {
        return (allowed & ConvertToAllowedRelation(relation)) != 0;
    }

    /// <summary>
    /// Converts a TemporalRelation enum to its corresponding AllowedRelations flag.
    /// </summary>
    internal static AllowedRelations ConvertToAllowedRelation(TemporalRelation relation)
    {
        return relation switch
        {
            TemporalRelation.Before => AllowedRelations.Before,
            TemporalRelation.Meets => AllowedRelations.Meets,
            TemporalRelation.Overlaps => AllowedRelations.Overlaps,
            TemporalRelation.Starts => AllowedRelations.Starts,
            TemporalRelation.During => AllowedRelations.During,
            TemporalRelation.Finishes => AllowedRelations.Finishes,
            TemporalRelation.Equal => AllowedRelations.Equal,
            TemporalRelation.After => AllowedRelations.After,
            TemporalRelation.MetBy => AllowedRelations.MetBy,
            TemporalRelation.OverlappedBy => AllowedRelations.OverlappedBy,
            TemporalRelation.StartedBy => AllowedRelations.StartedBy,
            TemporalRelation.Contains => AllowedRelations.Contains,
            TemporalRelation.FinishedBy => AllowedRelations.FinishedBy,
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "Unknown temporal relation")
        };
    }

    #endregion
}

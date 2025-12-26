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

using System.Buffers;

namespace Rebels.Temporal.Tests.Reference;

/// <summary>
/// Reference implementation of temporal matching algorithms for testing purposes.
/// </summary>
/// <remarks>
/// <para>
/// This class provides straightforward, brute-force implementations of all matching
/// strategies. It serves as the "source of truth" for validating source-generated code.
/// </para>
/// <para>
/// Performance is NOT a concern here - correctness and clarity are paramount.
/// This implementation is intentionally simple and fully tested to serve as a
/// reference for verifying that generated code produces identical results.
/// </para>
/// </remarks>
internal static class ReferenceTemporalMatcher
{
    #region Point-to-Point Pair Matching

    public static void MatchPointToPoint<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
    {
        var anchorTolerance = TPolicy.AnchorTolerance;
        var candidateTolerance = TPolicy.CandidateTolerance;

        // Determine matching strategy based on tolerances
        bool anchorHasTolerance = !anchorTolerance.IsExact;
        bool candidateHasTolerance = !candidateTolerance.IsExact;

        if (!anchorHasTolerance && !candidateHasTolerance)
        {
            MatchPointToPointExact(anchors, candidates, visitor);
        }
        else if (anchorHasTolerance && !candidateHasTolerance)
        {
            MatchPointToPointAnchorWindow(anchors, candidates, anchorTolerance, visitor);
        }
        else if (!anchorHasTolerance && candidateHasTolerance)
        {
            MatchPointToPointCandidateWindow(anchors, candidates, candidateTolerance, visitor);
        }
        else // Both have tolerance
        {
            MatchPointToPointBothWindows(anchors, candidates, anchorTolerance, candidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
        }
    }

    private static void MatchPointToPointExact<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var hasMatch = false;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                if (anchorTime == candidateTime)
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.PointExact);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    private static void MatchPointToPointAnchorWindow<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var hasMatch = false;

            var windowStart = anchorTime - anchorTolerance.Before;
            var windowEnd = anchorTime + anchorTolerance.After;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                if (candidateTime >= windowStart && candidateTime <= windowEnd)
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.PointInInterval);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    private static void MatchPointToPointCandidateWindow<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance candidateTolerance,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var hasMatch = false;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                var windowStart = candidateTime - candidateTolerance.Before;
                var windowEnd = candidateTime + candidateTolerance.After;

                if (anchorTime >= windowStart && anchorTime <= windowEnd)
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.PointInInterval);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    private static void MatchPointToPointBothWindows<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var hasMatch = false;

            var anchorStart = anchorTime - anchorTolerance.Before;
            var anchorEnd = anchorTime + anchorTolerance.After;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                var candidateStart = candidateTime - candidateTolerance.Before;
                var candidateEnd = candidateTime + candidateTolerance.After;

                var relation = DetermineAllenRelation(
                    anchorStart, anchorEnd, candidateStart, candidateEnd);

                if (IsRelationAllowed(relation, allowedRelations))
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.Interval, relation);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    #endregion

    #region Point-to-Interval Pair Matching

    public static void MatchPointToInterval<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
    {
        var anchorTolerance = TPolicy.AnchorTolerance;
        var candidateTolerance = TPolicy.CandidateTolerance;

        if (!anchorTolerance.IsExact)
        {
            MatchPointToIntervalWithAnchorTolerance(anchors, candidates, anchorTolerance, candidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
        }
        else
        {
            MatchPointToIntervalExact(anchors, candidates, candidateTolerance, visitor);
        }
    }

    private static void MatchPointToIntervalExact<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance candidateTolerance,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var hasMatch = false;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];

                var candidateStart = candidate.Start - candidateTolerance.Before;
                var candidateEnd = candidate.End + candidateTolerance.After;

                if (anchorTime >= candidateStart && anchorTime <= candidateEnd)
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.PointInInterval);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    private static void MatchPointToIntervalWithAnchorTolerance<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var anchorTime = anchor.At;
            var hasMatch = false;

            var anchorStart = anchorTime - anchorTolerance.Before;
            var anchorEnd = anchorTime + anchorTolerance.After;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];

                var candidateStart = candidate.Start - candidateTolerance.Before;
                var candidateEnd = candidate.End + candidateTolerance.After;

                var relation = DetermineAllenRelation(
                    anchorStart, anchorEnd, candidateStart, candidateEnd);

                if (IsRelationAllowed(relation, allowedRelations))
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.Interval, relation);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    #endregion

    #region Interval-to-Point Pair Matching

    public static void MatchIntervalToPoint<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
    {
        var anchorTolerance = TPolicy.AnchorTolerance;
        var candidateTolerance = TPolicy.CandidateTolerance;

        if (!candidateTolerance.IsExact)
        {
            MatchIntervalToPointWithCandidateTolerance(anchors, candidates, anchorTolerance, candidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
        }
        else
        {
            MatchIntervalToPointExact(anchors, candidates, anchorTolerance, visitor);
        }
    }

    private static void MatchIntervalToPointExact<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var hasMatch = false;

            var anchorStart = anchor.Start - anchorTolerance.Before;
            var anchorEnd = anchor.End + anchorTolerance.After;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                if (candidateTime >= anchorStart && candidateTime <= anchorEnd)
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.PointInInterval);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    private static void MatchIntervalToPointWithCandidateTolerance<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var hasMatch = false;

            var anchorStart = anchor.Start - anchorTolerance.Before;
            var anchorEnd = anchor.End + anchorTolerance.After;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];
                var candidateTime = candidate.At;

                var candidateStart = candidateTime - candidateTolerance.Before;
                var candidateEnd = candidateTime + candidateTolerance.After;

                var relation = DetermineAllenRelation(
                    anchorStart, anchorEnd, candidateStart, candidateEnd);

                if (IsRelationAllowed(relation, allowedRelations))
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.Interval, relation);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    #endregion

    #region Interval-to-Interval Pair Matching

    public static void MatchIntervalToInterval<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
    {
        MatchIntervalToIntervalImpl(anchors, candidates, TPolicy.AnchorTolerance, TPolicy.CandidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
    }

    private static void MatchIntervalToIntervalImpl<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IPairMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
    {
        for (int i = 0; i < anchors.Length; i++)
        {
            var anchor = anchors[i];
            var hasMatch = false;

            var anchorStart = anchor.Start - anchorTolerance.Before;
            var anchorEnd = anchor.End + anchorTolerance.After;

            for (int j = 0; j < candidates.Length; j++)
            {
                var candidate = candidates[j];

                var candidateStart = candidate.Start - candidateTolerance.Before;
                var candidateEnd = candidate.End + candidateTolerance.After;

                var relation = DetermineAllenRelation(
                    anchorStart, anchorEnd, candidateStart, candidateEnd);

                if (IsRelationAllowed(relation, allowedRelations))
                {
                    var pair = new MatchPair<TAnchor, TCandidate>(
                        anchor, candidate, MatchType.Interval, relation);
                    visitor.OnMatch(in pair);
                    hasMatch = true;
                }
            }

            if (!hasMatch)
                visitor.OnMiss(anchor);
        }
    }

    #endregion

    #region Point-to-Point Group Matching

    public static void MatchPointToPointGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
    {
        var anchorTolerance = TPolicy.AnchorTolerance;
        var candidateTolerance = TPolicy.CandidateTolerance;

        bool anchorHasTolerance = !anchorTolerance.IsExact;
        bool candidateHasTolerance = !candidateTolerance.IsExact;

        if (!anchorHasTolerance && !candidateHasTolerance)
        {
            MatchPointToPointExactGrouped(anchors, candidates, visitor);
        }
        else if (anchorHasTolerance && !candidateHasTolerance)
        {
            MatchPointToPointAnchorWindowGrouped(anchors, candidates, anchorTolerance, visitor);
        }
        else if (!anchorHasTolerance && candidateHasTolerance)
        {
            MatchPointToPointCandidateWindowGrouped(anchors, candidates, candidateTolerance, visitor);
        }
        else
        {
            MatchPointToPointBothWindowsGrouped(anchors, candidates, anchorTolerance, candidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
        }
    }

    private static void MatchPointToPointExactGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var anchorTime = anchor.At;
                int matchCount = 0;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var candidateTime = candidate.At;

                    if (anchorTime == candidateTime)
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    private static void MatchPointToPointAnchorWindowGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var anchorTime = anchor.At;
                int matchCount = 0;

                var windowStart = anchorTime - anchorTolerance.Before;
                var windowEnd = anchorTime + anchorTolerance.After;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var candidateTime = candidate.At;

                    if (candidateTime >= windowStart && candidateTime <= windowEnd)
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    private static void MatchPointToPointCandidateWindowGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance candidateTolerance,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var anchorTime = anchor.At;
                int matchCount = 0;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var candidateTime = candidate.At;

                    var windowStart = candidateTime - candidateTolerance.Before;
                    var windowEnd = candidateTime + candidateTolerance.After;

                    if (anchorTime >= windowStart && anchorTime <= windowEnd)
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    private static void MatchPointToPointBothWindowsGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalPoint
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var anchorTime = anchor.At;
                int matchCount = 0;

                var anchorStart = anchorTime - anchorTolerance.Before;
                var anchorEnd = anchorTime + anchorTolerance.After;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var candidateTime = candidate.At;

                    var candidateStart = candidateTime - candidateTolerance.Before;
                    var candidateEnd = candidateTime + candidateTolerance.After;

                    var relation = DetermineAllenRelation(
                        anchorStart, anchorEnd, candidateStart, candidateEnd);

                    if (IsRelationAllowed(relation, allowedRelations))
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    #endregion

    #region Point-to-Interval Group Matching

    public static void MatchPointToIntervalGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
    {
        var anchorTolerance = TPolicy.AnchorTolerance;
        var candidateTolerance = TPolicy.CandidateTolerance;

        if (!anchorTolerance.IsExact)
        {
            MatchPointToIntervalWithAnchorToleranceGrouped(anchors, candidates, anchorTolerance, candidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
        }
        else
        {
            MatchPointToIntervalExactGrouped(anchors, candidates, candidateTolerance, visitor);
        }
    }

    private static void MatchPointToIntervalExactGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance candidateTolerance,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var anchorTime = anchor.At;
                int matchCount = 0;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];

                    var candidateStart = candidate.Start - candidateTolerance.Before;
                    var candidateEnd = candidate.End + candidateTolerance.After;

                    if (anchorTime >= candidateStart && anchorTime <= candidateEnd)
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    private static void MatchPointToIntervalWithAnchorToleranceGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalPoint
        where TCandidate : ITemporalInterval
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                var anchorTime = anchor.At;
                int matchCount = 0;

                var anchorStart = anchorTime - anchorTolerance.Before;
                var anchorEnd = anchorTime + anchorTolerance.After;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];

                    var candidateStart = candidate.Start - candidateTolerance.Before;
                    var candidateEnd = candidate.End + candidateTolerance.After;

                    var relation = DetermineAllenRelation(
                        anchorStart, anchorEnd, candidateStart, candidateEnd);

                    if (IsRelationAllowed(relation, allowedRelations))
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    #endregion

    #region Interval-to-Point Group Matching

    public static void MatchIntervalToPointGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
        where TPolicy : IMatchPolicy
    {
        var anchorTolerance = TPolicy.AnchorTolerance;
        var candidateTolerance = TPolicy.CandidateTolerance;

        if (!candidateTolerance.IsExact)
        {
            MatchIntervalToPointWithCandidateToleranceGrouped(anchors, candidates, anchorTolerance, candidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
        }
        else
        {
            MatchIntervalToPointExactGrouped(anchors, candidates, anchorTolerance, visitor);
        }
    }

    private static void MatchIntervalToPointExactGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                int matchCount = 0;

                var anchorStart = anchor.Start - anchorTolerance.Before;
                var anchorEnd = anchor.End + anchorTolerance.After;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var candidateTime = candidate.At;

                    if (candidateTime >= anchorStart && candidateTime <= anchorEnd)
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    private static void MatchIntervalToPointWithCandidateToleranceGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalPoint
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                int matchCount = 0;

                var anchorStart = anchor.Start - anchorTolerance.Before;
                var anchorEnd = anchor.End + anchorTolerance.After;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];
                    var candidateTime = candidate.At;

                    var candidateStart = candidateTime - candidateTolerance.Before;
                    var candidateEnd = candidateTime + candidateTolerance.After;

                    var relation = DetermineAllenRelation(
                        anchorStart, anchorEnd, candidateStart, candidateEnd);

                    if (IsRelationAllowed(relation, allowedRelations))
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    #endregion

    #region Interval-to-Interval Group Matching

    public static void MatchIntervalToIntervalGrouped<TAnchor, TCandidate, TPolicy>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
        where TPolicy : IMatchPolicy
    {
        MatchIntervalToIntervalImplGrouped(anchors, candidates, TPolicy.AnchorTolerance, TPolicy.CandidateTolerance, TPolicy.AllowedTemporalRelations, visitor);
    }

    private static void MatchIntervalToIntervalImplGrouped<TAnchor, TCandidate>(
        ReadOnlySpan<TAnchor> anchors,
        ReadOnlySpan<TCandidate> candidates,
        TimeTolerance anchorTolerance,
        TimeTolerance candidateTolerance,
        AllowedRelations allowedRelations,
        IGroupMatchVisitor<TAnchor, TCandidate> visitor)
        where TAnchor : ITemporalInterval
        where TCandidate : ITemporalInterval
    {
        var matchBuffer = ArrayPool<TCandidate>.Shared.Rent(candidates.Length);
        try
        {
            for (int i = 0; i < anchors.Length; i++)
            {
                var anchor = anchors[i];
                int matchCount = 0;

                var anchorStart = anchor.Start - anchorTolerance.Before;
                var anchorEnd = anchor.End + anchorTolerance.After;

                for (int j = 0; j < candidates.Length; j++)
                {
                    var candidate = candidates[j];

                    var candidateStart = candidate.Start - candidateTolerance.Before;
                    var candidateEnd = candidate.End + candidateTolerance.After;

                    var relation = DetermineAllenRelation(
                        anchorStart, anchorEnd, candidateStart, candidateEnd);

                    if (IsRelationAllowed(relation, allowedRelations))
                    {
                        matchBuffer[matchCount++] = candidate;
                    }
                }

                if (matchCount > 0)
                {
                    var matchArray = new TCandidate[matchCount];
                    Array.Copy(matchBuffer, 0, matchArray, 0, matchCount);
                    var group = new MatchGroup<TAnchor, TCandidate>(anchor, matchArray, matchCount);
                    visitor.OnMatch(in group);
                }
                else
                {
                    visitor.OnMiss(anchor);
                }
            }
        }
        finally
        {
            ArrayPool<TCandidate>.Shared.Return(matchBuffer);
        }
    }

    #endregion

    #region Helper Methods

    private static TemporalRelation DetermineAllenRelation(
        DateTimeOffset anchorStart,
        DateTimeOffset anchorEnd,
        DateTimeOffset candidateStart,
        DateTimeOffset candidateEnd)
    {
        if (anchorStart == candidateStart && anchorEnd == candidateEnd)
            return TemporalRelation.Equal;

        if (anchorEnd == candidateStart)
            return TemporalRelation.Meets;
        if (anchorStart == candidateEnd)
            return TemporalRelation.MetBy;

        if (anchorEnd < candidateStart)
            return TemporalRelation.Before;
        if (anchorStart > candidateEnd)
            return TemporalRelation.After;

        if (anchorStart == candidateStart && anchorEnd < candidateEnd)
            return TemporalRelation.Starts;
        if (anchorStart == candidateStart && anchorEnd > candidateEnd)
            return TemporalRelation.StartedBy;

        if (anchorEnd == candidateEnd && anchorStart > candidateStart)
            return TemporalRelation.Finishes;
        if (anchorEnd == candidateEnd && anchorStart < candidateStart)
            return TemporalRelation.FinishedBy;

        if (anchorStart > candidateStart && anchorEnd < candidateEnd)
            return TemporalRelation.During;
        if (anchorStart < candidateStart && anchorEnd > candidateEnd)
            return TemporalRelation.Contains;

        if (anchorStart < candidateStart && anchorEnd < candidateEnd)
            return TemporalRelation.Overlaps;
        if (anchorStart > candidateStart && anchorEnd > candidateEnd)
            return TemporalRelation.OverlappedBy;

        throw new InvalidOperationException(
            $"Unable to determine temporal relation: Anchor=[{anchorStart}, {anchorEnd}], Candidate=[{candidateStart}, {candidateEnd}]");
    }

    private static bool IsRelationAllowed(TemporalRelation relation, AllowedRelations allowedRelations)
    {
        var relationFlag = relation switch
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
            _ => throw new ArgumentOutOfRangeException(nameof(relation))
        };

        return (allowedRelations & relationFlag) != 0;
    }

    #endregion
}

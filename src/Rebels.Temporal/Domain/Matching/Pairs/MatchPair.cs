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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rebels.Temporal;

/// <summary>
/// Immutable, allocation-free representation of a temporal match
/// between an anchor event and a candidate event.
/// 
/// This structure may describe:
/// - point-to-point exact matches
/// - point-to-window matches
/// - window-to-point matches
/// - window-to-window matches
/// - interval-to-interval matches (Allen's algebra)
/// </summary>
public readonly struct MatchPair<TAnchor, TCandidate>
{
    /// <summary>
    /// The anchor event for which the match was found.
    /// </summary>
    public TAnchor Anchor { get; }

    /// <summary>
    /// The candidate event that matched the anchor.
    /// </summary>
    public TCandidate Candidate { get; }

    /// <summary>
    /// Indicates how the match was computed (exact, window-based, interval-based).
    /// </summary>
    public MatchType MatchType { get; }

    /// <summary>
    /// Optional temporal relation describing how the intervals relate
    /// (only applies when MatchType = IntervalRelation).
    /// </summary>
    public TemporalRelation? Relation { get; }

    public MatchPair(
        TAnchor anchor,
        TCandidate candidate,
        MatchType matchType,
        TemporalRelation? relation = null)
    {
        Anchor = anchor;
        Candidate = candidate;
        MatchType = matchType;
        Relation = relation;
    }
}
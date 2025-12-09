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

namespace Rebels.Temporal.Domain.Matching;

/// <summary>
/// Describes the matching semantics applied to produce a match pair.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// Exact timestamp equality (point-to-point, no tolerance).
    /// </summary>
    Exact,

    /// <summary>
    /// The anchor is expanded into a time window; candidates are points.
    /// </summary>
    AnchorWindow,

    /// <summary>
    /// Candidates represent time windows; anchor is a point.
    /// </summary>
    CandidateWindow,

    /// <summary>
    /// Both anchor and candidate were interpreted as windows.
    /// </summary>
    BidirectionalWindow,

    /// <summary>
    /// Both sides are true intervals; temporal relation is computed
    /// according to Allen's interval algebra.
    /// </summary>
    IntervalRelation
}
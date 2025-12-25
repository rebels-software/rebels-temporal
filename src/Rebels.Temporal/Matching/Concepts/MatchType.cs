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

namespace Rebels.Temporal;

/// <summary>
/// Describes the type of temporal matching that occurred between
/// an anchor and a candidate event.
/// </summary>
public enum MatchType
{
    /// <summary>
    /// Both anchor and candidate represent points in time,
    /// and their timestamps must match exactly (no tolerance).
    /// </summary>
    PointExact,

    /// <summary>
    /// One side (anchor or candidate) is a point in time,
    /// and the other side represents an interval. The match
    /// occurs when the point lies within the interval.
    /// </summary>
    PointInInterval,

    /// <summary>
    /// Both anchor and candidate were interpreted as intervals.
    /// Their relationship is evaluated using Allen's Interval Algebra.
    /// </summary>
    Interval
}
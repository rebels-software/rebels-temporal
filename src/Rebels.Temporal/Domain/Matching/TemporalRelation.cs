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
/// Represents the thirteen fundamental temporal relations
/// between two intervals, as defined by Allen's Interval Algebra.
/// 
/// Each relation describes how an anchor interval A is positioned
/// relative to a candidate interval B.
/// </summary>
public enum TemporalRelation
{
    /// <summary>
    /// A ends before B starts.
    /// 
    /// A: [-----]
    /// B:           [-----]
    /// </summary>
    Before,

    /// <summary>
    /// A ends exactly when B starts.
    /// 
    /// A: [-----]
    /// B:       [-----]
    /// </summary>
    Meets,

    /// <summary>
    /// A starts before B and ends inside B.
    /// 
    /// A:   [-----]
    /// B: [-----------]
    /// </summary>
    Overlaps,

    /// <summary>
    /// A and B start together, but A ends earlier.
    /// 
    /// A: [-----]
    /// B: [-----------]
    /// </summary>
    Starts,

    /// <summary>
    /// A starts after B begins and ends before B finishes.
    /// 
    /// A:    [-----]
    /// B: [-----------]
    /// </summary>
    During,

    /// <summary>
    /// A ends together with B, but A starts later.
    /// 
    /// A:     [-----]
    /// B: [-----------]
    /// </summary>
    Finishes,

    /// <summary>
    /// A and B start and end at the same time.
    /// 
    /// A: [-----]
    /// B: [-----]
    /// </summary>
    Equal,

    /// <summary>
    /// A starts after B ends (the inverse of Before).
    /// 
    /// A:           [-----]
    /// B: [-----]
    /// </summary>
    After,

    /// <summary>
    /// A starts exactly when B ends (the inverse of Meets).
    /// 
    /// A:       [-----]
    /// B: [-----]
    /// </summary>
    MetBy,

    /// <summary>
    /// A starts inside B and ends after B finishes (inverse of Overlaps).
    /// 
    /// A:     [-----------]
    /// B: [-----]
    /// </summary>
    OverlappedBy,

    /// <summary>
    /// A and B start together, but A ends later (inverse of Starts).
    /// 
    /// A: [-----------]
    /// B: [-----]
    /// </summary>
    StartedBy,

    /// <summary>
    /// A starts before B and ends after B finishes (inverse of During).
    /// 
    /// A: [-----------]
    /// B:    [-----]
    /// </summary>
    Contains,

    /// <summary>
    /// A starts before B but ends together with B (inverse of Finishes).
    /// 
    /// A: [-----------]
    /// B:     [-----]
    /// </summary>
    FinishedBy
}
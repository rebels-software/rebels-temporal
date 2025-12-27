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
    /// Anchor ends before Candidate starts.
    /// 
    /// Anchor:    [-----]
    /// Candidate:           [-----]
    /// </summary>
    Before,

    /// <summary>
    /// Anchor ends exactly when Candidate starts.
    /// 
    /// Anchor:    [-----]
    /// Candidate:       [-----]
    /// </summary>
    Meets,

    /// <summary>
    /// Anchor starts before Candidate and ends inside Candidate.
    /// 
    /// Anchor:    [-----]
    /// Candidate:   [-----------]
    /// </summary>
    Overlaps,

    /// <summary>
    /// Anchor and Candidate start together, but Anchor ends earlier.
    /// 
    /// Anchor:    [-----]
    /// Candidate: [-----------]
    /// </summary>
    Starts,

    /// <summary>
    /// Anchor starts after Candidate begins and ends before Candidate finishes.
    /// 
    /// Anchor:       [-----]
    /// Candidate: [-----------]
    /// </summary>
    During,

    /// <summary>
    /// Anchor ends together with Candidate, but Anchor starts later.
    /// 
    /// Anchor:          [-----]
    /// Candidate: [-----------]
    /// </summary>
    Finishes,

    /// <summary>
    /// Anchor and Candidate start and end at the same time.
    /// 
    /// Anchor:    [-----]
    /// Candidate: [-----]
    /// </summary>
    Equal,

    /// <summary>
    /// Anchor starts after Candidate ends (the inverse of Before).
    /// 
    /// Anchor:              [-----]
    /// Candidate: [-----]
    /// </summary>
    After,

    /// <summary>
    /// Anchor starts exactly when Candidate ends (the inverse of Meets).
    /// 
    /// Anchor:          [-----]
    /// Candidate: [-----]
    /// </summary>
    MetBy,

    /// <summary>
    /// Anchor starts inside Candidate and ends after Candidate finishes (inverse of Overlaps).
    /// 
    /// Anchor:        [-----------]
    /// Candidate: [-----]
    /// </summary>
    OverlappedBy,

    /// <summary>
    /// Anchor and Candidate start together, but Anchor ends later (inverse of Starts).
    /// 
    /// Anchor:    [-----------]
    /// Candidate: [-----]
    /// </summary>
    StartedBy,

    /// <summary>
    /// Anchor starts before Candidate and ends after Candidate finishes (inverse of During).
    /// 
    /// Anchor:    [-----------]
    /// Candidate:    [-----]
    /// </summary>
    Contains,

    /// <summary>
    /// Anchor starts before Candidate but ends together with Candidate (inverse of Finishes).
    /// 
    /// Anchor:  [-----------]
    /// Candidate:     [-----]
    /// </summary>
    FinishedBy
}
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
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----]
    /// Candidate:           [-----]
    /// </code>
    /// </remarks>
    Before,

    /// <summary>
    /// Anchor ends exactly when Candidate starts.
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----]
    /// Candidate:       [-----]
    /// </code>
    /// </remarks>
    Meets,

    /// <summary>
    /// Anchor starts before Candidate and ends inside Candidate.
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----]
    /// Candidate:   [-----------]
    /// </code>
    /// </remarks>
    Overlaps,

    /// <summary>
    /// Anchor and Candidate start together, but Anchor ends earlier.
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----]
    /// Candidate: [-----------]
    /// </code>
    /// </remarks>
    Starts,

    /// <summary>
    /// Anchor starts after Candidate begins and ends before Candidate finishes.
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:       [-----]
    /// Candidate: [-----------]
    /// </code>
    /// </remarks>
    During,

    /// <summary>
    /// Anchor ends together with Candidate, but Anchor starts later.
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:          [-----]
    /// Candidate: [-----------]
    /// </code>
    /// </remarks>
    Finishes,

    /// <summary>
    /// Anchor and Candidate start and end at the same time.
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----]
    /// Candidate: [-----]
    /// </code>
    /// </remarks>
    Equal,

    /// <summary>
    /// Anchor starts after Candidate ends (the inverse of Before).
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:              [-----]
    /// Candidate: [-----]
    /// </code>
    /// </remarks>
    After,

    /// <summary>
    /// Anchor starts exactly when Candidate ends (the inverse of Meets).
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:          [-----]
    /// Candidate: [-----]
    /// </code>
    /// </remarks>
    MetBy,

    /// <summary>
    /// Anchor starts inside Candidate and ends after Candidate finishes (inverse of Overlaps).
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:        [-----------]
    /// Candidate: [-----]
    /// </code>
    /// </remarks>
    OverlappedBy,

    /// <summary>
    /// Anchor and Candidate start together, but Anchor ends later (inverse of Starts).
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----------]
    /// Candidate: [-----]
    /// </code>
    /// </remarks>
    StartedBy,

    /// <summary>
    /// Anchor starts before Candidate and ends after Candidate finishes (inverse of During).
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:    [-----------]
    /// Candidate:    [-----]
    /// </code>
    /// </remarks>
    Contains,

    /// <summary>
    /// Anchor starts before Candidate but ends together with Candidate (inverse of Finishes).
    /// </summary>
    /// <remarks>
    /// <code>
    /// Anchor:  [-----------]
    /// Candidate:     [-----]
    /// </code>
    /// </remarks>
    FinishedBy
}
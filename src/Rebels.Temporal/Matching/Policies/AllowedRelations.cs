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
/// Defines which temporal relations from Allen's Interval Algebra are allowed
/// during interval-based matching operations.
/// </summary>
/// <remarks>
/// This enum allows flexible filtering of interval matches based on their temporal relationship.
/// Multiple relations can be combined using bitwise OR operations.
/// Based on Allen's Interval Algebra, providing 13 mutually exclusive relations.
/// </remarks>
[Flags]
public enum AllowedRelations
{
    /// <summary>
    /// No relations are allowed. Matching will produce no results.
    /// </summary>
    None = 0,

    /// <summary>
    /// Anchor ends before Candidate starts.
    ///
    /// Anchor:    [-----]
    /// Candidate:           [-----]
    /// </summary>
    Before = 1 << 0,

    /// <summary>
    /// Anchor ends exactly when Candidate starts.
    ///
    /// Anchor:    [-----]
    /// Candidate:       [-----]
    /// </summary>
    Meets = 1 << 1,

    /// <summary>
    /// Anchor starts before Candidate and ends inside Candidate.
    ///
    /// Anchor:    [-----]
    /// Candidate:   [-----------]
    /// </summary>
    Overlaps = 1 << 2,

    /// <summary>
    /// Anchor and Candidate start together, but Anchor ends earlier.
    ///
    /// Anchor:    [-----]
    /// Candidate: [-----------]
    /// </summary>
    Starts = 1 << 3,

    /// <summary>
    /// Anchor starts after Candidate begins and ends before Candidate finishes.
    ///
    /// Anchor:       [-----]
    /// Candidate: [-----------]
    /// </summary>
    During = 1 << 4,

    /// <summary>
    /// Anchor ends together with Candidate, but Anchor starts later.
    ///
    /// Anchor:          [-----]
    /// Candidate: [-----------]
    /// </summary>
    Finishes = 1 << 5,

    /// <summary>
    /// Anchor and Candidate start and end at the same time.
    ///
    /// Anchor:    [-----]
    /// Candidate: [-----]
    /// </summary>
    Equal = 1 << 6,

    /// <summary>
    /// Anchor starts after Candidate ends (the inverse of Before).
    ///
    /// Anchor:              [-----]
    /// Candidate: [-----]
    /// </summary>
    After = 1 << 7,

    /// <summary>
    /// Anchor starts exactly when Candidate ends (the inverse of Meets).
    ///
    /// Anchor:          [-----]
    /// Candidate: [-----]
    /// </summary>
    MetBy = 1 << 8,

    /// <summary>
    /// Anchor starts inside Candidate and ends after Candidate finishes (inverse of Overlaps).
    ///
    /// Anchor:        [-----------]
    /// Candidate: [-----]
    /// </summary>
    OverlappedBy = 1 << 9,

    /// <summary>
    /// Anchor and Candidate start together, but Anchor ends later (inverse of Starts).
    ///
    /// Anchor:    [-----------]
    /// Candidate: [-----]
    /// </summary>
    StartedBy = 1 << 10,

    /// <summary>
    /// Anchor starts before Candidate and ends after Candidate finishes (inverse of During).
    ///
    /// Anchor:    [-----------]
    /// Candidate:    [-----]
    /// </summary>
    Contains = 1 << 11,

    /// <summary>
    /// Anchor starts before Candidate but ends together with Candidate (inverse of Finishes).
    ///
    /// Anchor:  [-----------]
    /// Candidate:     [-----]
    /// </summary>
    FinishedBy = 1 << 12,

    /// <summary>
    /// All temporal relations are allowed.
    /// </summary>
    Any = Before | Meets | Overlaps | Starts | During | Finishes | Equal |
          After | MetBy | OverlappedBy | StartedBy | Contains | FinishedBy
}

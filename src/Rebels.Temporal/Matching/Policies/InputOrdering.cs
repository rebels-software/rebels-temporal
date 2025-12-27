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
/// Specifies which input collections are guaranteed to be sorted
/// in ascending temporal order.
/// </summary>
/// <remarks>
/// This information is used at compile time by source generators
/// to select and generate the most efficient matching algorithm.
///
/// The ordering applies to the collections as they are passed
/// to the matcher method (anchors first, candidates second).
/// </remarks>
public enum InputOrdering
{
    /// <summary>
    /// Neither anchors nor candidates are sorted.
    /// The matcher must assume arbitrary order.
    /// </summary>
    None,

    /// <summary>
    /// Only the candidate collection is sorted
    /// in ascending temporal order.
    /// </summary>
    Candidates,

    /// <summary>
    /// Both anchor and candidate collections are sorted
    /// in ascending temporal order.
    /// </summary>
    Both
}

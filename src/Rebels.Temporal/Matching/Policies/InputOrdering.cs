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
/// Specifies which input collections are guaranteed to be sorted
/// in ascending temporal order.
/// </summary>
/// <remarks>
/// This information is used at runtime to select the most efficient
/// matching algorithm. When both collections are sorted, O(n+m) dual-pointer
/// algorithms are used. When only candidates are sorted, O(n log m) binary
/// search is used. Otherwise, O(n×m) nested loops are used.
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

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

namespace Rebels.Temporal.Domain.Matching
{
    /// <summary>
    /// Represents an entity that can be matched using a single timestamp.
    /// </summary>
    /// <remarks>
    /// Types implementing this interface expose exactly one point in time
    /// which can be used in point-to-point or point-to-interval matching operations.
    /// </remarks>
    public interface IMatchableByPoint
    {
        /// <summary>
        /// Gets the timestamp representing the temporal position of the entity.
        /// </summary>
        /// <remarks>
        /// This value must be expressed as a <see cref="DateTimeOffset"/> to ensure
        /// an unambiguous, timezone-aware moment in time.
        /// </remarks>
        DateTimeOffset At { get; }
    }
}
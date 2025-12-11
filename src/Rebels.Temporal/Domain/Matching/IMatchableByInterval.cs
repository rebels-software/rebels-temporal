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
    /// Represents an entity that can be matched using a time interval
    /// defined by a start and an end timestamp.
    /// </summary>
    /// <remarks>
    /// Implementations must guarantee that <see cref="Start"/> is less than
    /// or equal to <see cref="End"/>. Both values must be unambiguous moments
    /// in time expressed as <see cref="DateTimeOffset"/>.
    /// </remarks>
    public interface IMatchableByInterval
    {
        /// <summary>
        /// Gets the timestamp representing the start of the interval.
        /// </summary>
        DateTimeOffset Start { get; }

        /// <summary>
        /// Gets the timestamp representing the end of the interval.
        /// </summary>
        DateTimeOffset End { get; }
    }
}
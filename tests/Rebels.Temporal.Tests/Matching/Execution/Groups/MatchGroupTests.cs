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

using Rebels.Temporal.Tests.TestData;

namespace Rebels.Temporal.Tests.Matching.Execution.Groups;

[TestFixture]
public class MatchGroupTests
{
    [Test]
    public void Constructor_Should_Store_Anchor_And_Candidates()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[]
        {
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 2, TimeSpan.Zero), "C3")
        };

        // Act
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 3);

        // Assert
        Assert.That(group.Anchor, Is.EqualTo(anchor));
        Assert.That(group.Count, Is.EqualTo(3));
    }

    [Test]
    public void Constructor_With_Partial_Array_Should_Only_Expose_Count_Elements()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new TestEvent[10]; // Large array
        candidates[0] = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1");
        candidates[1] = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2");

        // Act
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 2); // Only first 2 are valid

        // Assert
        Assert.That(group.Count, Is.EqualTo(2));
        Assert.That(group.Matches.Length, Is.EqualTo(2));
        Assert.That(group[0].Name, Is.EqualTo("C1"));
        Assert.That(group[1].Name, Is.EqualTo("C2"));
    }

    [Test]
    public void Constructor_With_Empty_Matches_Should_Create_Group_With_Zero_Count()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = Array.Empty<TestEvent>();

        // Act
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 0);

        // Assert
        Assert.That(group.Count, Is.EqualTo(0));
        Assert.That(group.Matches.Length, Is.EqualTo(0));
    }

    [Test]
    public void Constructor_With_Null_Array_Should_Throw_ArgumentNullException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new MatchGroup<TestEvent, TestEvent>(anchor, null!, 0));

        Assert.That(exception!.ParamName, Is.EqualTo("matches"));
        Assert.That(exception.Message, Does.Contain("cannot be null"));
    }

    [Test]
    public void Constructor_With_Negative_Count_Should_Throw_ArgumentOutOfRangeException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new TestEvent[5];

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MatchGroup<TestEvent, TestEvent>(anchor, candidates, -1));

        Assert.That(exception!.ParamName, Is.EqualTo("count"));
        Assert.That(exception.Message, Does.Contain("cannot be negative"));
    }

    [Test]
    public void Constructor_With_Count_Exceeding_Array_Length_Should_Throw_ArgumentOutOfRangeException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new TestEvent[5];

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 10));

        Assert.That(exception!.ParamName, Is.EqualTo("count"));
        Assert.That(exception.Message, Does.Contain("cannot exceed matches array length"));
        Assert.That(exception.Message, Does.Contain("10"));
        Assert.That(exception.Message, Does.Contain("5"));
    }

    [Test]
    public void Indexer_Should_Return_Correct_Candidate()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[]
        {
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "First"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "Second"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 2, TimeSpan.Zero), "Third")
        };
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 3);

        // Act & Assert
        Assert.That(group[0].Name, Is.EqualTo("First"));
        Assert.That(group[1].Name, Is.EqualTo("Second"));
        Assert.That(group[2].Name, Is.EqualTo("Third"));
    }

    [Test]
    public void Indexer_With_Negative_Index_Should_Throw_IndexOutOfRangeException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[] { new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1") };
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 1);

        // Act & Assert
        var exception = Assert.Throws<IndexOutOfRangeException>(() => { var x = group[-1]; });

        Assert.That(exception!.Message, Does.Contain("-1"));
        Assert.That(exception.Message, Does.Contain("out of range"));
    }

    [Test]
    public void Indexer_With_Index_Equal_To_Count_Should_Throw_IndexOutOfRangeException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[]
        {
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2")
        };
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 2);

        // Act & Assert
        var exception = Assert.Throws<IndexOutOfRangeException>(() => { var x = group[2]; });

        Assert.That(exception!.Message, Does.Contain("2"));
        Assert.That(exception.Message, Does.Contain("out of range"));
    }

    [Test]
    public void Indexer_With_Index_Beyond_Count_Should_Throw_IndexOutOfRangeException()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[] { new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1") };
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 1);

        // Act & Assert
        var exception = Assert.Throws<IndexOutOfRangeException>(() => { var x = group[10]; });

        Assert.That(exception!.Message, Does.Contain("10"));
        Assert.That(exception.Message, Does.Contain("out of range"));
    }

    [Test]
    public void Matches_Should_Return_ReadOnlySpan_With_Correct_Length()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[]
        {
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 2, TimeSpan.Zero), "C3")
        };
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 3);

        // Act
        var span = group.Matches;

        // Assert
        Assert.That(span.Length, Is.EqualTo(3));
        Assert.That(span[0].Name, Is.EqualTo("C1"));
        Assert.That(span[1].Name, Is.EqualTo("C2"));
        Assert.That(span[2].Name, Is.EqualTo("C3"));
    }

    [Test]
    public void Matches_With_Partial_Array_Should_Only_Include_Count_Elements()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new TestEvent[10];
        candidates[0] = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1");
        candidates[1] = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2");
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 2);

        // Act
        var span = group.Matches;

        // Assert
        Assert.That(span.Length, Is.EqualTo(2));
        Assert.That(span[0].Name, Is.EqualTo("C1"));
        Assert.That(span[1].Name, Is.EqualTo("C2"));
    }

    [Test]
    public void ToArray_Should_Return_Copy_Of_Candidates()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[]
        {
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1"),
            new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2")
        };
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 2);

        // Act
        var array = group.ToArray();

        // Assert
        Assert.That(array.Length, Is.EqualTo(2));
        Assert.That(array[0].Name, Is.EqualTo("C1"));
        Assert.That(array[1].Name, Is.EqualTo("C2"));
        Assert.That(array, Is.Not.SameAs(candidates)); // Different array instance
    }

    [Test]
    public void ToArray_With_Empty_Group_Should_Return_Empty_Array()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = Array.Empty<TestEvent>();
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 0);

        // Act
        var array = group.ToArray();

        // Assert
        Assert.That(array.Length, Is.EqualTo(0));
        Assert.That(array, Is.SameAs(Array.Empty<TestEvent>())); // Should reuse singleton
    }

    [Test]
    public void ToArray_With_Partial_Array_Should_Only_Copy_Count_Elements()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new TestEvent[10];
        candidates[0] = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "C1");
        candidates[1] = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 1, TimeSpan.Zero), "C2");
        var group = new MatchGroup<TestEvent, TestEvent>(anchor, candidates, 2);

        // Act
        var array = group.ToArray();

        // Assert
        Assert.That(array.Length, Is.EqualTo(2));
        Assert.That(array[0].Name, Is.EqualTo("C1"));
        Assert.That(array[1].Name, Is.EqualTo("C2"));
    }

    [Test]
    public void MatchGroup_Should_Support_Different_Generic_Types()
    {
        // Arrange
        var anchor = new TestEvent(new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero), "Anchor");
        var candidates = new[]
        {
            new TestInterval(
                new DateTimeOffset(2025, 1, 1, 12, 0, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 1, 1, 12, 1, 0, TimeSpan.Zero),
                "Interval1"),
            new TestInterval(
                new DateTimeOffset(2025, 1, 1, 12, 2, 0, TimeSpan.Zero),
                new DateTimeOffset(2025, 1, 1, 12, 3, 0, TimeSpan.Zero),
                "Interval2")
        };

        // Act
        var group = new MatchGroup<TestEvent, TestInterval>(anchor, candidates, 2);

        // Assert
        Assert.That(group.Anchor.Name, Is.EqualTo("Anchor"));
        Assert.That(group[0].Name, Is.EqualTo("Interval1"));
        Assert.That(group[1].Name, Is.EqualTo("Interval2"));
    }

    [Test]
    public void MatchGroup_Should_Be_Readonly_Struct()
    {
        // This is a compile-time check, but we can verify the type is indeed a struct
        var type = typeof(MatchGroup<TestEvent, TestEvent>);

        Assert.That(type.IsValueType, Is.True);
    }
}

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

namespace Rebels.Temporal.Tests.Domain.Matching.Options;

[TestFixture]
public class TimeToleranceTests
{
    [Test]
    public void Constructor_Should_Store_Before_And_After_Values()
    {
        // Arrange
        var before = TimeSpan.FromSeconds(5);
        var after = TimeSpan.FromSeconds(10);

        // Act
        var tolerance = new TimeTolerance(before, after);

        // Assert
        Assert.That(tolerance.Before, Is.EqualTo(before));
        Assert.That(tolerance.After, Is.EqualTo(after));
    }

    [Test]
    public void Constructor_With_Zero_Values_Should_Create_Valid_Tolerance()
    {
        // Arrange & Act
        var tolerance = new TimeTolerance(TimeSpan.Zero, TimeSpan.Zero);

        // Assert
        Assert.That(tolerance.Before, Is.EqualTo(TimeSpan.Zero));
        Assert.That(tolerance.After, Is.EqualTo(TimeSpan.Zero));
        Assert.That(tolerance.IsExact, Is.True);
    }

    [Test]
    public void Constructor_With_Negative_Before_Should_Throw_ArgumentException()
    {
        // Arrange
        var negativeBefore = TimeSpan.FromSeconds(-5);
        var validAfter = TimeSpan.FromSeconds(10);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new TimeTolerance(negativeBefore, validAfter));

        Assert.That(exception!.Message, Does.Contain("Before tolerance cannot be negative"));
        Assert.That(exception.ParamName, Is.EqualTo("before"));
    }

    [Test]
    public void Constructor_With_Negative_After_Should_Throw_ArgumentException()
    {
        // Arrange
        var validBefore = TimeSpan.FromSeconds(5);
        var negativeAfter = TimeSpan.FromSeconds(-10);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new TimeTolerance(validBefore, negativeAfter));

        Assert.That(exception!.Message, Does.Contain("After tolerance cannot be negative"));
        Assert.That(exception.ParamName, Is.EqualTo("after"));
    }

    [Test]
    public void Constructor_With_Both_Negative_Should_Throw_ArgumentException()
    {
        // Arrange
        var negativeBefore = TimeSpan.FromSeconds(-5);
        var negativeAfter = TimeSpan.FromSeconds(-10);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new TimeTolerance(negativeBefore, negativeAfter));
    }

    [Test]
    public void Symmetric_Should_Create_Tolerance_With_Same_Before_And_After()
    {
        // Arrange
        var tolerance = TimeSpan.FromSeconds(7);

        // Act
        var result = TimeTolerance.Symmetric(tolerance);

        // Assert
        Assert.That(result.Before, Is.EqualTo(tolerance));
        Assert.That(result.After, Is.EqualTo(tolerance));
    }

    [Test]
    public void Symmetric_With_Zero_Should_Create_Exact_Tolerance()
    {
        // Act
        var result = TimeTolerance.Symmetric(TimeSpan.Zero);

        // Assert
        Assert.That(result.Before, Is.EqualTo(TimeSpan.Zero));
        Assert.That(result.After, Is.EqualTo(TimeSpan.Zero));
        Assert.That(result.IsExact, Is.True);
    }

    [Test]
    public void Symmetric_With_Negative_Value_Should_Throw_ArgumentException()
    {
        // Arrange
        var negativeTolerance = TimeSpan.FromSeconds(-5);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            TimeTolerance.Symmetric(negativeTolerance));
    }

    [Test]
    public void None_Should_Return_Zero_Tolerance()
    {
        // Act
        var tolerance = TimeTolerance.None;

        // Assert
        Assert.That(tolerance.Before, Is.EqualTo(TimeSpan.Zero));
        Assert.That(tolerance.After, Is.EqualTo(TimeSpan.Zero));
        Assert.That(tolerance.IsExact, Is.True);
    }

    [Test]
    public void IsExact_Should_Return_True_When_Both_Are_Zero()
    {
        // Arrange
        var tolerance = new TimeTolerance(TimeSpan.Zero, TimeSpan.Zero);

        // Act & Assert
        Assert.That(tolerance.IsExact, Is.True);
    }

    [Test]
    public void IsExact_Should_Return_False_When_Before_Is_NonZero()
    {
        // Arrange
        var tolerance = new TimeTolerance(TimeSpan.FromSeconds(1), TimeSpan.Zero);

        // Act & Assert
        Assert.That(tolerance.IsExact, Is.False);
    }

    [Test]
    public void IsExact_Should_Return_False_When_After_Is_NonZero()
    {
        // Arrange
        var tolerance = new TimeTolerance(TimeSpan.Zero, TimeSpan.FromSeconds(1));

        // Act & Assert
        Assert.That(tolerance.IsExact, Is.False);
    }

    [Test]
    public void IsExact_Should_Return_False_When_Both_Are_NonZero()
    {
        // Arrange
        var tolerance = new TimeTolerance(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));

        // Act & Assert
        Assert.That(tolerance.IsExact, Is.False);
    }

    [Test]
    public void TimeTolerance_Should_Support_Asymmetric_Values()
    {
        // Arrange
        var before = TimeSpan.FromSeconds(30);
        var after = TimeSpan.FromSeconds(5);

        // Act
        var tolerance = new TimeTolerance(before, after);

        // Assert
        Assert.That(tolerance.Before, Is.EqualTo(before));
        Assert.That(tolerance.After, Is.EqualTo(after));
        Assert.That(tolerance.Before, Is.Not.EqualTo(tolerance.After));
    }

    [Test]
    public void TimeTolerance_Should_Support_Large_TimeSpan_Values()
    {
        // Arrange
        var before = TimeSpan.FromDays(365);
        var after = TimeSpan.FromHours(48);

        // Act
        var tolerance = new TimeTolerance(before, after);

        // Assert
        Assert.That(tolerance.Before, Is.EqualTo(TimeSpan.FromDays(365)));
        Assert.That(tolerance.After, Is.EqualTo(TimeSpan.FromHours(48)));
    }

    [Test]
    public void TimeTolerance_Should_Support_Millisecond_Precision()
    {
        // Arrange
        var before = TimeSpan.FromMilliseconds(250);
        var after = TimeSpan.FromMilliseconds(500);

        // Act
        var tolerance = new TimeTolerance(before, after);

        // Assert
        Assert.That(tolerance.Before, Is.EqualTo(TimeSpan.FromMilliseconds(250)));
        Assert.That(tolerance.After, Is.EqualTo(TimeSpan.FromMilliseconds(500)));
    }

    [Test]
    public void TimeTolerance_Should_Be_Readonly_Struct()
    {
        // This is a compile-time check, but we can verify the type is indeed a struct
        var type = typeof(TimeTolerance);

        Assert.That(type.IsValueType, Is.True);
    }

    [TestCase(0, 0, true)]
    [TestCase(1, 0, false)]
    [TestCase(0, 1, false)]
    [TestCase(5, 10, false)]
    [TestCase(100, 100, false)]
    public void IsExact_Should_Return_Expected_Value(int beforeSeconds, int afterSeconds, bool expectedIsExact)
    {
        // Arrange
        var tolerance = new TimeTolerance(
            TimeSpan.FromSeconds(beforeSeconds),
            TimeSpan.FromSeconds(afterSeconds));

        // Act & Assert
        Assert.That(tolerance.IsExact, Is.EqualTo(expectedIsExact));
    }
}

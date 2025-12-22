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
public class AllowedRelationsTests
{
    [Test]
    public void AllowedRelations_Should_Have_Same_Elements_As_TemporalRelation_Except_None_And_Any()
    {
        // Arrange
        var temporalRelationValues = Enum.GetValues<TemporalRelation>()
            .Select(r => r.ToString())
            .OrderBy(name => name)
            .ToList();

        var allowedRelationsValues = Enum.GetValues<AllowedRelations>()
            .Where(r => r != AllowedRelations.None && r != AllowedRelations.Any)
            .Select(r => r.ToString())
            .OrderBy(name => name)
            .ToList();

        // Act & Assert
        Assert.That(allowedRelationsValues, Is.EqualTo(temporalRelationValues),
            "AllowedRelations should contain exactly the same relation names as TemporalRelation (excluding None and Any)");
    }

    [Test]
    public void AllowedRelations_Should_Have_None_With_Value_Zero()
    {
        // Act & Assert
        Assert.That((int)AllowedRelations.None, Is.EqualTo(0));
    }

    [Test]
    public void AllowedRelations_Should_Have_Unique_Bit_Flags()
    {
        // Arrange
        var allValues = Enum.GetValues<AllowedRelations>()
            .Where(r => r != AllowedRelations.None && r != AllowedRelations.Any)
            .Select(r => (int)r)
            .ToList();

        // Act - check that each value is a power of 2 (single bit set)
        var arePowersOfTwo = allValues.All(v => v > 0 && (v & (v - 1)) == 0);

        // Assert
        Assert.That(arePowersOfTwo, Is.True,
            "All AllowedRelations values (except None and Any) should be powers of 2 (single bit flags)");
    }

    [Test]
    public void AllowedRelations_Any_Should_Include_All_Relations()
    {
        // Arrange
        var allIndividualRelations = Enum.GetValues<AllowedRelations>()
            .Where(r => r != AllowedRelations.None && r != AllowedRelations.Any)
            .ToList();

        // Act & Assert
        foreach (var relation in allIndividualRelations)
        {
            Assert.That((AllowedRelations.Any & relation) == relation, Is.True,
                $"AllowedRelations.Any should include {relation}");
        }
    }

    [Test]
    public void AllowedRelations_Can_Be_Combined_With_Bitwise_OR()
    {
        // Act
        var combined = AllowedRelations.Before | AllowedRelations.After | AllowedRelations.Equal;

        // Assert
        Assert.That((combined & AllowedRelations.Before) == AllowedRelations.Before, Is.True);
        Assert.That((combined & AllowedRelations.After) == AllowedRelations.After, Is.True);
        Assert.That((combined & AllowedRelations.Equal) == AllowedRelations.Equal, Is.True);
        Assert.That((combined & AllowedRelations.During) == AllowedRelations.During, Is.False);
    }

    [TestCase(TemporalRelation.Before, AllowedRelations.Before)]
    [TestCase(TemporalRelation.Meets, AllowedRelations.Meets)]
    [TestCase(TemporalRelation.Overlaps, AllowedRelations.Overlaps)]
    [TestCase(TemporalRelation.Starts, AllowedRelations.Starts)]
    [TestCase(TemporalRelation.During, AllowedRelations.During)]
    [TestCase(TemporalRelation.Finishes, AllowedRelations.Finishes)]
    [TestCase(TemporalRelation.Equal, AllowedRelations.Equal)]
    [TestCase(TemporalRelation.After, AllowedRelations.After)]
    [TestCase(TemporalRelation.MetBy, AllowedRelations.MetBy)]
    [TestCase(TemporalRelation.OverlappedBy, AllowedRelations.OverlappedBy)]
    [TestCase(TemporalRelation.StartedBy, AllowedRelations.StartedBy)]
    [TestCase(TemporalRelation.Contains, AllowedRelations.Contains)]
    [TestCase(TemporalRelation.FinishedBy, AllowedRelations.FinishedBy)]
    public void Each_TemporalRelation_Should_Have_Corresponding_AllowedRelation(
        TemporalRelation temporal,
        AllowedRelations allowed)
    {
        // Assert - names should match
        Assert.That(temporal.ToString(), Is.EqualTo(allowed.ToString()));
    }

    [Test]
    public void AllowedRelations_Should_Have_Flags_Attribute()
    {
        // Arrange
        var type = typeof(AllowedRelations);

        // Act
        var hasFlagsAttribute = type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

        // Assert
        Assert.That(hasFlagsAttribute, Is.True,
            "AllowedRelations should have [Flags] attribute to support bitwise operations");
    }

    [Test]
    public void TemporalRelation_Should_Not_Have_Flags_Attribute()
    {
        // Arrange
        var type = typeof(TemporalRelation);

        // Act
        var hasFlagsAttribute = type.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

        // Assert
        Assert.That(hasFlagsAttribute, Is.False,
            "TemporalRelation should NOT have [Flags] attribute as it represents mutually exclusive relations");
    }

    [Test]
    public void AllowedRelations_Count_Should_Be_TemporalRelation_Count_Plus_Two()
    {
        // Arrange
        var temporalRelationCount = Enum.GetValues<TemporalRelation>().Length;
        var allowedRelationsCount = Enum.GetValues<AllowedRelations>().Length;

        // Act & Assert - AllowedRelations has all TemporalRelations + None + Any
        Assert.That(allowedRelationsCount, Is.EqualTo(temporalRelationCount + 2),
            "AllowedRelations should have same count as TemporalRelation plus None and Any");
    }
}

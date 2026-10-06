using System;
using System.Linq;
using FluentAssertions;
using KomTracker.Domain.Entities.Component;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Component;

public class ComponentCategoryMetadataTests
{
    [Fact]
    public void Every_category_maps_to_a_group()
    {
        foreach (var category in Enum.GetValues<ComponentCategory>())
        {
            var group = ComponentCategoryMetadata.Group(category);
            Enum.IsDefined(group).Should().BeTrue();
        }
    }

    [Fact]
    public void CategoriesByGroup_covers_all_categories_exactly_once()
    {
        var grouped = ComponentCategoryMetadata.CategoriesByGroup()
            .SelectMany(g => g.Categories)
            .ToList();

        grouped.Should().BeEquivalentTo(Enum.GetValues<ComponentCategory>());
        grouped.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(ComponentCategory.Rim, ComponentCategoryGroup.Wheels)]
    [InlineData(ComponentCategory.PowerMeter, ComponentCategoryGroup.Accessories)]
    public void Category_maps_to_expected_group(ComponentCategory category, ComponentCategoryGroup expected)
    {
        ComponentCategoryMetadata.Group(category).Should().Be(expected);
    }

    [Fact]
    public void PowerMeter_has_spaced_display_name()
    {
        ComponentCategoryMetadata.DisplayName(ComponentCategory.PowerMeter).Should().Be("Power Meter");
    }

    [Fact]
    public void Other_falls_back_to_accessories_group()
    {
        ComponentCategoryMetadata.Group(ComponentCategory.Other).Should().Be(ComponentCategoryGroup.Accessories);
    }
}

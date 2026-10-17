using System;
using System.Linq;
using FluentAssertions;
using KomTracker.Domain.Entities.Expense;
using Xunit;

namespace KomTracker.Application.Tests.Commands.Expense;

public class ExpenseCategoryMetadataTests
{
    [Fact]
    public void CategoriesByGroup_covers_all_categories_exactly_once()
    {
        var grouped = ExpenseCategoryMetadata.CategoriesByGroup().SelectMany(g => g.Categories).ToList();

        grouped.Should().BeEquivalentTo(Enum.GetValues<ExpenseCategory>());
        grouped.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Every_group_has_at_least_one_category()
        => ExpenseCategoryMetadata.CategoriesByGroup().Select(g => g.Group)
            .Should().BeEquivalentTo(Enum.GetValues<ExpenseCategoryGroup>());

    [Theory]
    [InlineData(ExpenseCategory.Cleats, ExpenseCategoryGroup.Clothing)]
    [InlineData(ExpenseCategory.Tubeless, ExpenseCategoryGroup.Tyres)]
    [InlineData(ExpenseCategory.Lubricants, ExpenseCategoryGroup.Workshop)]
    [InlineData(ExpenseCategory.Lights, ExpenseCategoryGroup.Electronics)]
    [InlineData(ExpenseCategory.Fenders, ExpenseCategoryGroup.Accessories)]
    [InlineData(ExpenseCategory.Trainer, ExpenseCategoryGroup.Training)]
    [InlineData(ExpenseCategory.Nutrition, ExpenseCategoryGroup.Nutrition)]
    [InlineData(ExpenseCategory.Other, ExpenseCategoryGroup.Other)]
    public void Category_maps_to_expected_group(ExpenseCategory category, ExpenseCategoryGroup expected)
        => ExpenseCategoryMetadata.Group(category).Should().Be(expected);
}

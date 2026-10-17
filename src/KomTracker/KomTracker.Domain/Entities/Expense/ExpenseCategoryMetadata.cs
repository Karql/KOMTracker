using System;
using System.Collections.Generic;
using System.Linq;

namespace KomTracker.Domain.Entities.Expense;

/// <summary>
/// Code-side metadata for <see cref="ExpenseCategory"/> (D-13 style) — the UI grouping and display names.
/// Mirrors <c>ComponentCategoryMetadata</c>.
/// </summary>
public static class ExpenseCategoryMetadata
{
    /// <summary>The <see cref="ExpenseCategoryGroup"/> a category belongs to.</summary>
    public static ExpenseCategoryGroup Group(ExpenseCategory category) => category switch
    {
        ExpenseCategory.Clothing or ExpenseCategory.Shoes or ExpenseCategory.Cleats
            or ExpenseCategory.Helmets or ExpenseCategory.Eyewear => ExpenseCategoryGroup.Clothing,

        ExpenseCategory.Tubes or ExpenseCategory.Tubeless => ExpenseCategoryGroup.Tyres,

        ExpenseCategory.Cables or ExpenseCategory.Parts => ExpenseCategoryGroup.Parts,

        ExpenseCategory.Tools or ExpenseCategory.Pumps or ExpenseCategory.Lubricants
            or ExpenseCategory.Cleaning => ExpenseCategoryGroup.Workshop,

        ExpenseCategory.Electronics or ExpenseCategory.Lights => ExpenseCategoryGroup.Electronics,

        ExpenseCategory.Bags or ExpenseCategory.Bottles or ExpenseCategory.Locks
            or ExpenseCategory.Fenders or ExpenseCategory.Accessories => ExpenseCategoryGroup.Accessories,

        ExpenseCategory.Trainer => ExpenseCategoryGroup.Training,

        ExpenseCategory.Nutrition => ExpenseCategoryGroup.Nutrition,

        _ => ExpenseCategoryGroup.Other
    };

    /// <summary>
    /// Categories grouped for the picker: groups in <see cref="ExpenseCategoryGroup"/> order, categories in
    /// <see cref="ExpenseCategory"/> declaration order within each group.
    /// </summary>
    public static IReadOnlyList<(ExpenseCategoryGroup Group, IReadOnlyList<ExpenseCategory> Categories)> CategoriesByGroup()
        => Enum.GetValues<ExpenseCategory>()
            .GroupBy(Group)
            .OrderBy(g => g.Key)
            .Select(g => (g.Key, (IReadOnlyList<ExpenseCategory>)g.ToList()))
            .ToList();

    /// <summary>Human-friendly category label (the single-word names read fine as they are).</summary>
    public static string DisplayName(ExpenseCategory category) => category.ToString();

    /// <summary>Human-friendly group label.</summary>
    public static string DisplayName(ExpenseCategoryGroup group) => group.ToString();
}

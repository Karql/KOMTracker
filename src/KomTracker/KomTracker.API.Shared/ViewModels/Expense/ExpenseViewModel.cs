using System;
using KomTracker.Domain.Entities.Expense;

namespace KomTracker.API.Shared.ViewModels.Expense;

/// <summary>A standalone cost entry (read model).</summary>
public class ExpenseViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ExpenseCategory Category { get; set; }

    /// <summary>UI group of <see cref="Category"/> (from the code-side registry) — for grouping/filters.</summary>
    public ExpenseCategoryGroup CategoryGroup { get; set; }

    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? PurchasePlace { get; set; }
    public string? Notes { get; set; }

    public ExpenseLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SalePlace { get; set; }
}

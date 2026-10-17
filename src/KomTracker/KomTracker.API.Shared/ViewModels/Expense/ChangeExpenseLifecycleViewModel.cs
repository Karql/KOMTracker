using System;
using KomTracker.Domain.Entities.Expense;

namespace KomTracker.API.Shared.ViewModels.Expense;

/// <summary>Request body for an expense lifecycle transition (sell / archive / restore).</summary>
public class ChangeExpenseLifecycleViewModel
{
    public ExpenseLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SalePlace { get; set; }

    /// <summary>Optional note to save with the transition (null = leave unchanged).</summary>
    public string? Notes { get; set; }
}

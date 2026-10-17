using System;
using KomTracker.Domain.Entities.Expense;

namespace KomTracker.API.Shared.ViewModels.Expense;

/// <summary>
/// Request body for creating/updating an expense (add + edit share this shape; WEB form binds to it).
/// Field names mirror <c>SaveExpenseCommand</c> (guarded by a parity test). Server-owned fields
/// (id/owner/lifecycle/audit) are not here.
/// </summary>
public class SaveExpenseViewModel
{
    public string Name { get; set; } = default!;
    public ExpenseCategory Category { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string? PurchasePlace { get; set; }
    public string? Notes { get; set; }

    /// <summary>Sale details — editable only while the expense is Sold (correct a forgotten/wrong sale).</summary>
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
    public string? SalePlace { get; set; }
}

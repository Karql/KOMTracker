#nullable enable
using System;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Expense;

/// <summary>
/// A standalone cost entry (BikeTracker) — clothing, tubes, lube, tools, electronics… anything bought that is not an
/// installable part (D-20). Owned by the platform User. Table: bt.expense. Not linked to a bike; no installations or
/// mileage. All dates are UTC (timestamptz).
/// </summary>
public class ExpenseEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Owner — FK to the identity user (AspNetUsers.Id). Scoping key.</summary>
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;

    public ExpenseCategory Category { get; set; }

    /// <summary>What it cost (in the user's currency; 0 allowed — e.g. a prize).</summary>
    public decimal Price { get; set; }

    /// <summary>When it was bought.</summary>
    public DateTime PurchaseDate { get; set; }

    /// <summary>Where it was bought (free text, shared with the purchase-place suggestions).</summary>
    public string? PurchasePlace { get; set; }

    public string? Notes { get; set; }

    public ExpenseLifecycle Lifecycle { get; set; } = ExpenseLifecycle.Active;

    /// <summary>Set when Lifecycle == Sold (kept when leaving Sold, so a re-sale starts pre-filled).</summary>
    public DateTime? SaleDate { get; set; }

    public decimal? SalePrice { get; set; }

    /// <summary>Where/to whom it was sold (free text).</summary>
    public string? SalePlace { get; set; }
}

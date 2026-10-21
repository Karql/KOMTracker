#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using KomTracker.Domain.Entities.Bike;
using KomTracker.Domain.Entities.Component;
using KomTracker.Domain.Entities.Expense;

namespace KomTracker.Domain.Entities.CostAnalysis;

/// <summary>
/// The cost ledger of one user (Phase 4, D-COST-1): every cost and resale already attributed to a bike, plus the km
/// needed for per-km figures. Built on the server, sliced / aggregated in the browser. A read model — it is also the
/// wire format (no 1:1 view-model copy, D-COST-9).
/// </summary>
public class CostAnalysisReport
{
    public List<CostLine> Lines { get; set; } = new();
    public List<CostBike> Bikes { get; set; } = new();
}

/// <summary>Where a cost comes from — the top level of the breakdown and the source toggles.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CostSource
{
    Bikes,
    Components,
    Services,
    Expenses
}

/// <summary>A cost (money out) or a resale (money back — negative amount).</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CostKind
{
    Cost,
    Resale
}

/// <summary>
/// One attributed ledger line. A multi-bike component, or a service covering several things, yields one line per bike
/// (<see cref="Share"/> of <see cref="FullAmount"/>). <see cref="Amount"/> is signed: resales are negative.
/// </summary>
public class CostLine
{
    /// <summary>When the money moved (purchase / service / expense / sale date); null = unknown (counts only in "All time").</summary>
    public DateTime? Date { get; set; }

    public CostSource Source { get; set; }
    public CostKind Kind { get; set; }

    /// <summary>Breakdown level 2 (component / expense group, "Bikes", "Services").</summary>
    public string Group { get; set; } = default!;

    /// <summary>Breakdown level 3 (category display name; bike type; what a service covered).</summary>
    public string Category { get; set; } = default!;

    /// <summary>For the icon of component / service lines.</summary>
    public ComponentCategory? ComponentCategory { get; set; }

    /// <summary>For the icon of expense lines.</summary>
    public ExpenseCategory? ExpenseCategory { get; set; }

    /// <summary>The bike / component / service / expense id (the link target, per <see cref="Source"/>).</summary>
    public int ItemId { get; set; }

    public string ItemName { get; set; } = default!;

    /// <summary>The bike this line is attributed to; null = no bike ("Not on a bike" for parts, "General" for expenses).</summary>
    public int? BikeId { get; set; }

    /// <summary>Fraction of the item's amount on this line (1 = all of it).</summary>
    public decimal Share { get; set; } = 1m;

    /// <summary>Signed attributed amount (resale negative).</summary>
    public decimal Amount { get; set; }

    /// <summary>The item's whole (unsigned) amount before the split.</summary>
    public decimal FullAmount { get; set; }

    /// <summary>Why this share (e.g. "3 120 of 5 030 km on this bike"); null when the line is the whole item.</summary>
    public string? Note { get; set; }
}

/// <summary>A bike as the analysis needs it: name / status for the filter, km for the per-km figures.</summary>
public class CostBike
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public BikeLifecycle Lifecycle { get; set; }

    /// <summary>The bike's initial km (from before tracking) — undated, counted in "All time" only (D-COST-6).</summary>
    public decimal SeedKm { get; set; }

    /// <summary>Ride km per calendar year (UTC start date).</summary>
    public Dictionary<int, decimal> KmByYear { get; set; } = new();
}

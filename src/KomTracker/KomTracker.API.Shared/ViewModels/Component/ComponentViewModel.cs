using System;
using KomTracker.Domain.Entities.Component;

namespace KomTracker.API.Shared.ViewModels.Component;

/// <summary>A component in the user's inventory (read model). No computed mileage in Phase 2a — Initial seed only.</summary>
public class ComponentViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public ComponentCategory Category { get; set; }

    /// <summary>UI group of <see cref="Category"/> (from the code-side registry) — for grouping/labels.</summary>
    public ComponentCategoryGroup CategoryGroup { get; set; }

    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public string? PurchasePlace { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal InitialDistanceKm { get; set; }
    public decimal? InitialMovingHours { get; set; }
    public decimal? InitialElevationM { get; set; }

    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    public ComponentLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
}

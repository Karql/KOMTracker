using System;
using KomTracker.API.Shared.ViewModels.Installation;
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

    /// <summary>Meta component = a container others can be installed into (only these are offered as install targets).</summary>
    public bool IsMetaComponent { get; set; }

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

    // Current placement (D-7): either in ONE parent component, or on one-or-more distinct bikes — never both.
    // Display priority: installed (bike(s) ▸ parent component) ▸ warehouse ▸ unassigned.

    /// <summary>First bike placement — convenience for single-bike UI.</summary>
    public int? InstalledOnBikeId { get; set; }
    public string? InstalledOnBikeName { get; set; }
    public InstallationPosition? InstalledPosition { get; set; }

    /// <summary>Number of distinct bikes it's currently on (0 when in a parent component or unassigned).</summary>
    public int InstalledBikeCount { get; set; }

    /// <summary>Parent component it's currently installed into (if any).</summary>
    public int? ParentComponentId { get; set; }
    public string? ParentComponentName { get; set; }

    /// <summary>All current active placements (bikes or the single parent component). Drives the location chip.</summary>
    public InstallationViewModel[] CurrentPlacements { get; set; } = Array.Empty<InstallationViewModel>();

    /// <summary>Installations of components INTO this one (current + historical, one level).</summary>
    public InstallationViewModel[] Children { get; set; } = Array.Empty<InstallationViewModel>();

    public ComponentLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }
}

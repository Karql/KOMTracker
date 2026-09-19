#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// A component in the user's inventory (BikeTracker) — a wearable/replaceable or cost-only part.
/// Owned by the platform User. Table: bt.component. All dates are UTC (timestamptz).
/// Mirrors <see cref="Bike.BikeEntity"/> (OQ-3: separate entity, no shared base). Installations + computed
/// mileage come later (Phase 2b / 3), so only the Initial seed metrics live here for now.
/// </summary>
public class ComponentEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Owner — FK to the identity user (AspNetUsers.Id). Scoping key.</summary>
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public ComponentCategory Category { get; set; }

    /// <summary>
    /// Meta component = a container that other components can be installed INTO (e.g. a wheel holding a tyre).
    /// Only meta components are offered as install targets; can't be turned off while it has installation history as a parent.
    /// </summary>
    public bool IsMetaComponent { get; set; }

    /// <summary>Optional weight in kilograms.</summary>
    public decimal? WeightKg { get; set; }

    public string? Notes { get; set; }

    // Purchase info
    public decimal? Price { get; set; }

    public string? PurchasePlace { get; set; }

    /// <summary>UTC. Only the date is shown in the UI.</summary>
    public DateTime? PurchaseDate { get; set; }

    // Initial (odometer) seed — the component's usage before tracking started.
    public decimal InitialDistanceKm { get; set; }

    public decimal? InitialMovingHours { get; set; }

    public decimal? InitialElevationM { get; set; }

    /// <summary>Current warehouse (where it sits when not installed). Nullable — FK to bt.warehouse (SetNull on delete).</summary>
    public int? WarehouseId { get; set; }

    // Lifecycle
    public ComponentLifecycle Lifecycle { get; set; } = ComponentLifecycle.Active;

    /// <summary>UTC. Set when Lifecycle == Sold.</summary>
    public DateTime? SaleDate { get; set; }

    public decimal? SalePrice { get; set; }

    /// <summary>Name of the current warehouse, for display; set by the component queries — NOT persisted.</summary>
    [NotMapped]
    public string? WarehouseName { get; set; }

    // Current active Tracked placement(s) (where it's mounted now), set by the component queries — NOT persisted.
    // Display priority: installed (on bike(s) ▸ in a parent component) ▸ warehouse ▸ unassigned.
    // A component is homogeneous (D-7): either in ONE parent component, or on one-or-more distinct bikes — never both.

    /// <summary>All current active Tracked placements (names resolved). Drives the location chip/field.</summary>
    [NotMapped]
    public IReadOnlyList<InstallationEntity> CurrentPlacements { get; set; } = Array.Empty<InstallationEntity>();

    /// <summary>The parent component this is currently installed into (if any). Mutually exclusive with bike placements.</summary>
    [NotMapped]
    public int? ParentComponentId { get; set; }

    [NotMapped]
    public string? ParentComponentName { get; set; }

    /// <summary>Number of distinct bikes this is currently installed on (0 when in a parent component or unassigned).</summary>
    [NotMapped]
    public int InstalledBikeCount { get; set; }

    /// <summary>Installations of components INTO this one (current + historical, one level; names resolved). Set by queries.</summary>
    [NotMapped]
    public IReadOnlyList<InstallationEntity> Children { get; set; } = Array.Empty<InstallationEntity>();

    // First bike placement — convenience/back-compat for single-bike UI (chip, location field).
    [NotMapped]
    public int? InstalledOnBikeId { get; set; }

    [NotMapped]
    public string? InstalledOnBikeName { get; set; }

    [NotMapped]
    public InstallationPosition? InstalledPosition { get; set; }

    // Computed mileage (Phase 3) — read from the bt.component_mileage projection by the component queries. NOT persisted here.
    [NotMapped]
    public decimal TotalDistanceKm { get; set; }

    [NotMapped]
    public decimal TotalMovingHours { get; set; }

    [NotMapped]
    public decimal TotalElevationM { get; set; }

    [NotMapped]
    public int AttributedActivityCount { get; set; }
}

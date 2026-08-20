#nullable enable
using System;
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
}

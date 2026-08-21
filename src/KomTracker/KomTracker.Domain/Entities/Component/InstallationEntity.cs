#nullable enable
using System;
using System.ComponentModel.DataAnnotations.Schema;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// An installation of a <see cref="ComponentEntity"/> onto a parent over a time window with a position.
/// Owned by the platform User. Table: bt.installation. All dates are UTC (timestamptz).
/// Phase 2b-i: parent is a Bike (<see cref="BikeId"/>); component-in-component (a ParentComponentId) comes in 2b-ii,
/// hence BikeId is nullable and the parent invariant is enforced in app logic.
/// </summary>
public class InstallationEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Owner — FK to the identity user (AspNetUsers.Id). Scoping key.</summary>
    public string UserId { get; set; } = default!;

    /// <summary>The installed component (FK to bt.component).</summary>
    public int ComponentId { get; set; }

    /// <summary>Parent bike (FK to bt.bike). Required in 2b-i; nullable for the 2b-ii component-parent case.</summary>
    public int? BikeId { get; set; }

    public ComponentInstallationType Type { get; set; }

    /// <summary>UTC. Set for <see cref="ComponentInstallationType.Tracked"/>; null for Manual.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>UTC. Null = currently installed (Tracked). Manual rows are always historical.</summary>
    public DateTime? DateTo { get; set; }

    public InstallationPosition? Position { get; set; }

    // Manual (historical) static totals — hand-entered, never computed. Manual type only.
    public decimal? ManualDistanceKm { get; set; }

    public decimal? ManualMovingHours { get; set; }

    public decimal? ManualElevationM { get; set; }

    // Read-model fields (set by queries) — NOT persisted.
    [NotMapped]
    public string? ComponentName { get; set; }

    [NotMapped]
    public ComponentCategory? ComponentCategory { get; set; }

    [NotMapped]
    public string? BikeName { get; set; }

    /// <summary>Currently installed = an active Tracked window (no DateTo). Manual is never "current".</summary>
    [NotMapped]
    public bool IsCurrent => Type == ComponentInstallationType.Tracked && DateTo is null;
}

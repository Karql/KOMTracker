#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Bike;

/// <summary>
/// A bike in the user's garage (BikeTracker). Owned by the platform User (not the Strava athlete),
/// so ownership survives future integrations (Garmin, etc.). Table: bt.bike. All dates are UTC (timestamptz).
/// </summary>
public class BikeEntity : BaseEntity
{
    public int Id { get; set; }

    /// <summary>Owner — FK to the identity user (AspNetUsers.Id). Scoping key.</summary>
    public string UserId { get; set; } = default!;

    public string Name { get; set; } = default!;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public BikeType Type { get; set; }

    /// <summary>Optional weight in kilograms.</summary>
    public decimal? WeightKg { get; set; }

    public string? Notes { get; set; }

    // Purchase info
    public decimal? Price { get; set; }

    public string? PurchasePlace { get; set; }

    /// <summary>UTC. Only the date is shown in the UI.</summary>
    public DateTime? PurchaseDate { get; set; }

    // Initial (odometer) seed — the bike's mileage/usage before tracking started.
    public decimal InitialDistanceKm { get; set; }

    public decimal? InitialMovingHours { get; set; }

    public decimal? InitialElevationM { get; set; }

    // Lifecycle
    public BikeLifecycle Lifecycle { get; set; } = BikeLifecycle.Active;

    /// <summary>UTC. Set when Lifecycle == Sold.</summary>
    public DateTime? SaleDate { get; set; }

    public decimal? SalePrice { get; set; }

    /// <summary>External-service links (bt.bike_link). Loaded on demand by queries — NOT an EF navigation
    /// (keeps the update path clean; bike_link is written only via its own repo).</summary>
    [NotMapped]
    public IReadOnlyList<BikeLinkEntity> Links { get; set; } = new List<BikeLinkEntity>();

    // Computed read-model totals (initial + Σ attributed activities), set by the bike queries — NOT persisted.
    // Default to the initial seed so a manual/unlinked bike shows its own numbers.

    [NotMapped]
    public decimal TotalDistanceKm { get; set; }

    [NotMapped]
    public decimal TotalMovingHours { get; set; }

    [NotMapped]
    public decimal TotalElevationM { get; set; }

    /// <summary>How many synced Strava activities are attributed to this bike (via its links).</summary>
    [NotMapped]
    public int AttributedActivityCount { get; set; }

    /// <summary>Name of the linked Strava bike (strava.bike), for display; set by the bike queries.</summary>
    [NotMapped]
    public string? StravaBikeName { get; set; }
}

#nullable enable
using System;
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
}

using KomTracker.Domain.Entities.Bike;

namespace KomTracker.API.Shared.ViewModels.Bike;

public class BikeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public BikeType Type { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Notes { get; set; }
    public decimal? Price { get; set; }
    public string? PurchasePlace { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal InitialDistanceKm { get; set; }
    public decimal? InitialMovingHours { get; set; }
    public decimal? InitialElevationM { get; set; }
    public BikeLifecycle Lifecycle { get; set; }
    public DateTime? SaleDate { get; set; }
    public decimal? SalePrice { get; set; }

    /// <summary>Linked Strava gear id (bt.bike_link), or null when the bike isn't linked to Strava.</summary>
    public string? StravaGearId { get; set; }

    // Computed mileage totals (initial + Σ attributed activities).
    public decimal TotalDistanceKm { get; set; }
    public decimal TotalMovingHours { get; set; }
    public decimal TotalElevationM { get; set; }
    public int AttributedActivityCount { get; set; }
}

using KomTracker.Domain.Entities.Bike;

namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>
/// A synced Strava bike (strava.bike) for the "Strava bikes" page. Carries a server-suggested
/// <see cref="SuggestedType"/> and km distance so the "Create bike" dialog can pre-fill, plus the
/// linked bt.bike (if any) for the "Linked" badge / deep-link.
/// </summary>
public class StravaBikeViewModel
{
    public string Id { get; set; } = default!;   // Strava gear id
    public string? Name { get; set; }
    public string? Nickname { get; set; }
    public bool Primary { get; set; }
    public bool Retired { get; set; }

    public double DistanceKm { get; set; }
    public string? BrandName { get; set; }
    public string? ModelName { get; set; }

    /// <summary>frame_type mapped to a BikeType (create pre-fill suggestion).</summary>
    public BikeType SuggestedType { get; set; }
    public decimal? WeightKg { get; set; }

    public int? LinkedBikeId { get; set; }
    public string? LinkedBikeName { get; set; }
}

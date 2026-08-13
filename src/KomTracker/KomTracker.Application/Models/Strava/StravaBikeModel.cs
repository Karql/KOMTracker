using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Application.Models.Strava;

/// <summary>A synced Strava bike plus the bt.bike it is linked to (if any).</summary>
public class StravaBikeModel
{
    public StravaBikeEntity Bike { get; set; } = default!;
    public int? LinkedBikeId { get; set; }
    public string? LinkedBikeName { get; set; }
}

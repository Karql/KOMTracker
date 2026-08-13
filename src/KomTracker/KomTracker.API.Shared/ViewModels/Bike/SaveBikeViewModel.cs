using KomTracker.Domain.Entities.Bike;

namespace KomTracker.API.Shared.ViewModels.Bike;

/// <summary>
/// Request body for creating/updating a bike (add + edit share this shape; WEB form binds to it).
/// Field names mirror <c>SaveBikeCommand</c> (guarded by a parity test) so validation error keys
/// line up with the posted JSON. Server-owned fields (id/owner/lifecycle/audit) are not here.
/// </summary>
public class SaveBikeViewModel
{
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

    /// <summary>When set on create, links the new bike to this Strava gear id (bt.bike_link). Ignored on update.</summary>
    public string? StravaGearId { get; set; }
}

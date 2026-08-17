namespace KomTracker.Application.Models.Strava;

/// <summary>Aggregated activity totals for one Strava gear id (source for a bike's mileage).</summary>
public class GearTotalsModel
{
    public string GearId { get; set; } = default!;
    public double DistanceMeters { get; set; }
    public long MovingTimeSeconds { get; set; }
    public double ElevationMeters { get; set; }
    public int ActivityCount { get; set; }
}

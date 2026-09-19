namespace KomTracker.Application.Models.Strava;

/// <summary>
/// Lightweight projected activity row for component-mileage attribution — enough to bucket a ride into an
/// installation window without loading the full <c>ActivityEntity</c>. Metrics in metres / seconds; StartDate is UTC.
/// </summary>
public class ActivityAttributionModel
{
    public string GearId { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public double DistanceMeters { get; set; }
    public int MovingTimeSeconds { get; set; }
    public double ElevationMeters { get; set; }
}

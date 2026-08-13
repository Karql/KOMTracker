#nullable enable
using KomTracker.Domain.Contracts;

namespace KomTracker.Domain.Entities.Strava;

/// <summary>
/// A Strava gear "bike", synced 1:1 from GET /athlete (bikes[]) hydrated by GET /gear/{id} (DetailedGear).
/// First-class Strava record (like <see cref="ActivityEntity"/>). Table: strava.bike.
/// Keyed by the Strava gear id (e.g. "b1234567"); carries Strava's own athlete_id.
/// Shoes are out of scope (a future strava.shoe). Materializing a bt.bike from this is user-initiated
/// (see bt.bike_link) — this table is just the raw mirror.
/// </summary>
public class StravaBikeEntity : BaseEntity
{
    public string Id { get; set; } = default!;   // Strava gear id
    public int AthleteId { get; set; }           // Strava athlete id (FK -> athlete)

    public string? Name { get; set; }
    public string? Nickname { get; set; }
    public bool Primary { get; set; }
    public bool Retired { get; set; }

    public double Distance { get; set; }          // metres
    public double ConvertedDistance { get; set; } // km (unit-dependent on Strava; stored for 1:1)

    // From DetailedGear (GET /gear/{id})
    public string? BrandName { get; set; }
    public string? ModelName { get; set; }
    public int? FrameType { get; set; }           // 1=mtb, 2=cross, 3=road, 4=TT (mapped to BikeType at create)
    public string? Description { get; set; }
    public double? Weight { get; set; }           // kg
}

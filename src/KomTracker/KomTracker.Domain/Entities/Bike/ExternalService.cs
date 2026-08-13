using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Bike;

/// <summary>
/// External service a bike can be linked to (bt.bike_link). Persisted by name (string).
/// Strava today; room for Garmin etc. later. <see cref="Other"/> is the escape hatch.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExternalService
{
    Strava,
    Other
}

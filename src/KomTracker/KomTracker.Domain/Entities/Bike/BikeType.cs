using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Bike;

/// <summary>
/// Kind of bike. Persisted by name (string) — see BikeEntityTypeConfiguration.
/// Serialized by name on the wire too (targeted converter, doesn't affect other enums).
/// Renaming a member requires a data migration (stored value = member name).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BikeType
{
    Road,
    Mountain,
    Gravel,
    Urban,
    Triathlon,
    Cyclocross,
    Hybrid,
    Indoor,
    Commuter,
    EBike,
    TimeTrial,
    Touring,
    BMX,
    Other
}

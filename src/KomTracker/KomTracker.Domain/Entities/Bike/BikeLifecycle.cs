using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Bike;

/// <summary>
/// Lifecycle state of a bike. Persisted and serialized by name (string).
/// Active bikes show in the garage by default; Archived/Sold are hidden unless requested.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BikeLifecycle
{
    Active,
    Archived,
    Sold
}

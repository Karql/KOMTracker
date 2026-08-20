using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// Lifecycle state of a component. Persisted and serialized by name (string).
/// Active components show by default; Archived/Sold are hidden unless requested.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentLifecycle
{
    Active,
    Archived,
    Sold
}

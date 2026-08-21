using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// Where a component sits on its parent (Installation.Position). Persisted and serialized by name (string).
/// Optional — many installations have no meaningful position.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InstallationPosition
{
    Front,
    Rear,
    Left,
    Right,
    Top,
    Bottom,
    Other
}

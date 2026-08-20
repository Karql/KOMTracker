using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// UI grouping for <see cref="ComponentCategory"/> (CONCEPT §14). Code-side only — used to render a grouped
/// category picker; not persisted on its own (a component stores its <see cref="ComponentCategory"/>).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentCategoryGroup
{
    Brakes,
    Drivetrain,
    Cockpit,
    Wheels,
    Structure,
    Suspension,
    Cables,
    Electric,
    Indoor,
    Accessories
}

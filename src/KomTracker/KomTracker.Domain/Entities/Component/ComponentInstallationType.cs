using System.Text.Json.Serialization;

namespace KomTracker.Domain.Entities.Component;

/// <summary>
/// How a component installation is tracked. Persisted and serialized by name (string).
/// <c>Tracked</c> = has a date window; mileage will be computed from activities (Phase 3).
/// <c>Manual</c> = dateless historical record with static, hand-entered totals; always historical.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentInstallationType
{
    Tracked,
    Manual
}

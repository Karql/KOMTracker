using Strava.API.Client.Model.Base;
using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Gear;

/// <summary>
/// Strava "SummaryGear" — a bike or shoe. Appears in the athlete's bikes[]/shoes[].
/// Field set from real responses (superset of the OpenAPI schema — nickname/retired/converted_distance
/// are undocumented but real). See docs/strava-api-notes.md.
/// </summary>
public class GearSummaryModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("resource_state")]
    public ResourceStateEnum ResourceState { get; set; }

    [JsonPropertyName("primary")]
    public bool Primary { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("retired")]
    public bool Retired { get; set; }

    /// <summary>Total distance in metres. `double` — cumulative gear distance exceeds float's ~7-digit precision.</summary>
    [JsonPropertyName("distance")]
    public double Distance { get; set; }

    /// <summary>Total distance in kilometres (Strava's convenience value).</summary>
    [JsonPropertyName("converted_distance")]
    public double ConvertedDistance { get; set; }
}

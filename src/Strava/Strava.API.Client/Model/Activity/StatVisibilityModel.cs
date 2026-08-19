using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Activity;

/// <summary>Per-stat visibility flag (DetailedActivity `stats_visibility`).</summary>
public class StatVisibilityModel
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; }
}

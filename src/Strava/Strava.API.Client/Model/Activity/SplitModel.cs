using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Activity;

/// <summary>A per-distance split (from DetailedActivity `splits_metric` / `splits_standard`).</summary>
public class SplitModel
{
    [JsonPropertyName("distance")]
    public float Distance { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("elevation_difference")]
    public float? ElevationDifference { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTime { get; set; }

    [JsonPropertyName("split")]
    public int Split { get; set; }

    [JsonPropertyName("average_speed")]
    public float AverageSpeed { get; set; }

    [JsonPropertyName("average_grade_adjusted_speed")]
    public float? AverageGradeAdjustedSpeed { get; set; }

    [JsonPropertyName("average_heartrate")]
    public float? AverageHeartrate { get; set; }

    [JsonPropertyName("pace_zone")]
    public int PaceZone { get; set; }
}

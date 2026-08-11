using Strava.API.Client.Model.Athlete;
using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Activity;

/// <summary>
/// A Strava "SummaryActivity" as returned by GET /athlete/activities.
/// Field set follows the real payload in docs/strava-api-notes.md (superset of the OpenAPI schema).
/// </summary>
public class ActivitySummaryModel : ActivityMetaModel
{
    [JsonPropertyName("athlete")]
    public AthleteMetaModel Athlete { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("external_id")]
    public string ExternalId { get; set; }

    [JsonPropertyName("upload_id")]
    public long? UploadId { get; set; }

    /// <summary>Links to a gear (bike/shoe) id; nullable / "none" when unset. No gear ⇒ unattributed.</summary>
    [JsonPropertyName("gear_id")]
    public string GearId { get; set; }

    // Metrics (metres / seconds)
    [JsonPropertyName("distance")]
    public float Distance { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTime { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("total_elevation_gain")]
    public float TotalElevationGain { get; set; }

    [JsonPropertyName("elev_high")]
    public float? ElevHigh { get; set; }

    [JsonPropertyName("elev_low")]
    public float? ElevLow { get; set; }

    // Type
    /// <remarks>Deprecated by Strava in favour of <see cref="SportType"/>.</remarks>
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("sport_type")]
    public string SportType { get; set; }

    [JsonPropertyName("workout_type")]
    public int? WorkoutType { get; set; }

    // Dates (D-15): start_date is the canonical UTC instant; start_date_local carries a bogus 'Z'
    // (local wall-clock, not UTC) — do NOT treat as an instant; local = start_date + utc_offset.
    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("start_date_local")]
    public DateTime StartDateLocal { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    /// <summary>Seconds, DST-correct. Hand-added — absent from the OpenAPI schema, present in real responses.</summary>
    [JsonPropertyName("utc_offset")]
    public float UtcOffset { get; set; }

    // Flags
    [JsonPropertyName("trainer")]
    public bool Trainer { get; set; }

    [JsonPropertyName("commute")]
    public bool Commute { get; set; }

    [JsonPropertyName("manual")]
    public bool Manual { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("flagged")]
    public bool Flagged { get; set; }

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; }

    // Speed / power / HR
    [JsonPropertyName("average_speed")]
    public float AverageSpeed { get; set; }

    [JsonPropertyName("max_speed")]
    public float MaxSpeed { get; set; }

    [JsonPropertyName("average_cadence")]
    public float? AverageCadence { get; set; }

    [JsonPropertyName("average_temp")]
    public int? AverageTemp { get; set; }

    [JsonPropertyName("average_watts")]
    public float? AverageWatts { get; set; }

    [JsonPropertyName("weighted_average_watts")]
    public float? WeightedAverageWatts { get; set; }

    [JsonPropertyName("max_watts")]
    public float? MaxWatts { get; set; }

    [JsonPropertyName("device_watts")]
    public bool DeviceWatts { get; set; }

    [JsonPropertyName("kilojoules")]
    public float? Kilojoules { get; set; }

    [JsonPropertyName("has_heartrate")]
    public bool HasHeartrate { get; set; }

    [JsonPropertyName("average_heartrate")]
    public float? AverageHeartrate { get; set; }

    [JsonPropertyName("max_heartrate")]
    public float? MaxHeartrate { get; set; }

    [JsonPropertyName("suffer_score")]
    public float? SufferScore { get; set; }

    // Counts
    [JsonPropertyName("achievement_count")]
    public int AchievementCount { get; set; }

    [JsonPropertyName("kudos_count")]
    public int KudosCount { get; set; }

    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }

    [JsonPropertyName("athlete_count")]
    public int AthleteCount { get; set; }

    [JsonPropertyName("photo_count")]
    public int PhotoCount { get; set; }

    [JsonPropertyName("total_photo_count")]
    public int TotalPhotoCount { get; set; }

    [JsonPropertyName("pr_count")]
    public int PrCount { get; set; }

    // Map / geo / device
    [JsonPropertyName("map")]
    public PolylineMapModel Map { get; set; }

    [JsonPropertyName("start_latlng")]
    public float[] StartLatlng { get; set; }

    [JsonPropertyName("end_latlng")]
    public float[] EndLatlng { get; set; }

    [JsonPropertyName("device_name")]
    public string DeviceName { get; set; }
}

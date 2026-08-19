using Strava.API.Client.Model.Athlete;
using Strava.API.Client.Model.Base;
using System;
using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Activity;

/// <summary>A lap (from DetailedActivity `laps`).</summary>
public class LapModel
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("resource_state")]
    public ResourceStateEnum ResourceState { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("activity")]
    public ActivityMetaModel Activity { get; set; }

    [JsonPropertyName("athlete")]
    public AthleteMetaModel Athlete { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTime { get; set; }

    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    /// <remarks>Local wall-clock returned with a bogus 'Z' — do not treat as an instant (same as the activity — D-15).</remarks>
    [JsonPropertyName("start_date_local")]
    public DateTime StartDateLocal { get; set; }

    [JsonPropertyName("distance")]
    public float Distance { get; set; }

    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("end_index")]
    public int EndIndex { get; set; }

    [JsonPropertyName("total_elevation_gain")]
    public float TotalElevationGain { get; set; }

    [JsonPropertyName("average_speed")]
    public float AverageSpeed { get; set; }

    [JsonPropertyName("max_speed")]
    public float MaxSpeed { get; set; }

    [JsonPropertyName("average_cadence")]
    public float? AverageCadence { get; set; }

    [JsonPropertyName("device_watts")]
    public bool DeviceWatts { get; set; }

    [JsonPropertyName("average_watts")]
    public float? AverageWatts { get; set; }

    [JsonPropertyName("average_heartrate")]
    public float? AverageHeartrate { get; set; }

    [JsonPropertyName("max_heartrate")]
    public float? MaxHeartrate { get; set; }

    [JsonPropertyName("lap_index")]
    public int LapIndex { get; set; }

    [JsonPropertyName("split")]
    public int Split { get; set; }

    [JsonPropertyName("pace_zone")]
    public int PaceZone { get; set; }
}

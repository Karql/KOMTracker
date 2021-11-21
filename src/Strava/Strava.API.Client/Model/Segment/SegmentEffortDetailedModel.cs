using Strava.API.Client.Model.Activity;
using Strava.API.Client.Model.Athlete;
using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Segment;

/// <remakrs>
/// Whole API use Detailed
/// even when resource_state show 2 is detailed :D
/// </remarks>
public class SegmentEffortDetailedModel
{
    [JsonPropertyName("resource_state")]
    public ResourceStateEnum ResourceState { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("activity")]
    public ActivityMetaModel Activity { get; set; }

    [JsonPropertyName("athlete")]
    public AthleteMetaModel Athlete { get; set; }

    [JsonPropertyName("segment")]
    public SegmentSummaryModel Segment { get; set; }

    /// <summary>
    /// Segment name
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("elapsed_time")]
    public int ElapsedTime { get; set; }

    [JsonPropertyName("moving_time")]
    public int MovingTime { get; set; }

    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }

    /// <remarks>
    /// Local but returned as UTC...
    /// </remarks>
    [JsonPropertyName("start_date_local")]
    public DateTime StartDateLocal { get; set; }

    [JsonPropertyName("distance")]
    public float Distance { get; set; }

    [JsonPropertyName("start_index")]
    public int StartIndex { get; set; }

    [JsonPropertyName("end_index")]
    public int EndIndex { get; set; }

    [JsonPropertyName("average_cadence")]
    public float AverageCadence { get; set; }

    [JsonPropertyName("device_watts")]
    public bool DeviceWatts { get; set; }

    [JsonPropertyName("average_watts")]
    public float AverageWatts { get; set; }

    [JsonPropertyName("average_heartrate")]
    public float AverageHeartrate { get; set; }

    [JsonPropertyName("max_heartrate")]
    public float MaxHeartrate { get; set; }

    [JsonPropertyName("pr_rank")]
    public int? PrRank { get; set; }

    [JsonPropertyName("kom_rank")]
    public int? KomRank { get; set; }
}

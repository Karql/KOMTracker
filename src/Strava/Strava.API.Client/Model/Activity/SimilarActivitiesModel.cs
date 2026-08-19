using Strava.API.Client.Model.Base;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Activity;

/// <summary>"How this ride compares to similar efforts" (DetailedActivity `similar_activities`).</summary>
public class SimilarActivitiesModel
{
    [JsonPropertyName("effort_count")]
    public int EffortCount { get; set; }

    [JsonPropertyName("average_speed")]
    public float AverageSpeed { get; set; }

    [JsonPropertyName("min_average_speed")]
    public float MinAverageSpeed { get; set; }

    [JsonPropertyName("mid_average_speed")]
    public float MidAverageSpeed { get; set; }

    [JsonPropertyName("max_average_speed")]
    public float MaxAverageSpeed { get; set; }

    [JsonPropertyName("pr_rank")]
    public int? PrRank { get; set; }

    [JsonPropertyName("frequency_milestone")]
    public string FrequencyMilestone { get; set; }

    [JsonPropertyName("resource_state")]
    public ResourceStateEnum ResourceState { get; set; }

    [JsonPropertyName("trend")]
    public ActivityTrendModel Trend { get; set; }
}

/// <summary>Speed trend across similar activities (DetailedActivity `similar_activities.trend`).</summary>
public class ActivityTrendModel
{
    [JsonPropertyName("speeds")]
    public List<float> Speeds { get; set; }

    [JsonPropertyName("current_activity_index")]
    public int CurrentActivityIndex { get; set; }

    [JsonPropertyName("min_speed")]
    public float MinSpeed { get; set; }

    [JsonPropertyName("mid_speed")]
    public float MidSpeed { get; set; }

    [JsonPropertyName("max_speed")]
    public float MaxSpeed { get; set; }

    [JsonPropertyName("direction")]
    public int Direction { get; set; }
}

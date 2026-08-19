using Strava.API.Client.Model.Gear;
using Strava.API.Client.Model.Segment;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Activity;

/// <summary>
/// A Strava "DetailedActivity" as returned by GET /activities/{id}. Superset of <see cref="ActivitySummaryModel"/>.
/// The client mirrors the full payload (universal connector); BikeTracker persists only the summary fields
/// (<see cref="ActivitySummaryModel"/> base), so the existing summary→entity mapping applies unchanged.
/// See docs/strava-api-notes.md.
/// </summary>
public class ActivityDetailedModel : ActivitySummaryModel
{
    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("calories")]
    public float? Calories { get; set; }

    [JsonPropertyName("perceived_exertion")]
    public float? PerceivedExertion { get; set; }

    [JsonPropertyName("prefer_perceived_exertion")]
    public bool? PreferPerceivedExertion { get; set; }

    [JsonPropertyName("hide_from_home")]
    public bool HideFromHome { get; set; }

    [JsonPropertyName("leaderboard_opt_out")]
    public bool LeaderboardOptOut { get; set; }

    [JsonPropertyName("segment_leaderboard_opt_out")]
    public bool? SegmentLeaderboardOptOut { get; set; }

    [JsonPropertyName("embed_token")]
    public string EmbedToken { get; set; }

    [JsonPropertyName("available_zones")]
    public List<string> AvailableZones { get; set; }

    [JsonPropertyName("gear")]
    public GearSummaryModel Gear { get; set; }

    [JsonPropertyName("segment_efforts")]
    public List<SegmentEffortDetailedModel> SegmentEfforts { get; set; }

    [JsonPropertyName("best_efforts")]
    public List<SegmentEffortDetailedModel> BestEfforts { get; set; }

    [JsonPropertyName("splits_metric")]
    public List<SplitModel> SplitsMetric { get; set; }

    [JsonPropertyName("splits_standard")]
    public List<SplitModel> SplitsStandard { get; set; }

    [JsonPropertyName("laps")]
    public List<LapModel> Laps { get; set; }

    [JsonPropertyName("photos")]
    public PhotosSummaryModel Photos { get; set; }

    [JsonPropertyName("similar_activities")]
    public SimilarActivitiesModel SimilarActivities { get; set; }

    [JsonPropertyName("stats_visibility")]
    public List<StatVisibilityModel> StatsVisibility { get; set; }
}

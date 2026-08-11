using Strava.API.Client.Model.Club;
using Strava.API.Client.Model.Gear;
using System.Text.Json.Serialization;

namespace Strava.API.Client.Model.Athlete;

/// <summary>
/// Strava "DetailedAthlete" as returned by GET /athlete (resource_state 3).
/// Full field set from the real response (superset of the spec; see docs/strava-api-notes.md).
/// weight/bio/username live on <see cref="AthleteSummaryModel"/> because the token-exchange athlete carries them.
/// BikeTracker only needs bikes[]; the rest is modelled so the connector reflects what the API actually returns.
/// </summary>
public class AthleteDetailedModel : AthleteSummaryModel
{
    [JsonPropertyName("blocked")]
    public bool Blocked { get; set; }

    [JsonPropertyName("can_follow")]
    public bool CanFollow { get; set; }

    [JsonPropertyName("follower_count")]
    public int FollowerCount { get; set; }

    [JsonPropertyName("friend_count")]
    public int FriendCount { get; set; }

    [JsonPropertyName("mutual_friend_count")]
    public int MutualFriendCount { get; set; }

    [JsonPropertyName("athlete_type")]
    public int AthleteType { get; set; }

    [JsonPropertyName("date_preference")]
    public string DatePreference { get; set; }

    [JsonPropertyName("measurement_preference")]
    public string MeasurementPreference { get; set; }

    [JsonPropertyName("clubs")]
    public ClubSummaryModel[] Clubs { get; set; }

    [JsonPropertyName("postable_clubs_count")]
    public int PostableClubsCount { get; set; }

    [JsonPropertyName("ftp")]
    public int? Ftp { get; set; }

    [JsonPropertyName("bikes")]
    public GearSummaryModel[] Bikes { get; set; }

    [JsonPropertyName("shoes")]
    public GearSummaryModel[] Shoes { get; set; }
}

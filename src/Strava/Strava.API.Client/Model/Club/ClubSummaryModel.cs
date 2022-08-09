using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Club;
public class ClubSummaryModel
{
    [JsonPropertyName("resource_state")]
    public ResourceStateEnum ResourceState { get; set; }

    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("profile_medium")]
    public string ProfileMedium { get; set; }

    [JsonPropertyName("profile")]
    public string Profile { get; set; }

    [JsonPropertyName("cover_photo")]
    public string CoverPhoto { get; set; }

    [JsonPropertyName("cover_photo_small")]
    public string CoverPhotoSmall { get; set; }

    [JsonPropertyName("activity_types")]
    public string[] ActivityTypes { get; set; }

    [JsonPropertyName("activity_types_icon")]
    public string ActivityTypesIcon { get; set; }

    [JsonPropertyName("dimensions")]
    public string[] Dimensions { get; set; }

    [JsonPropertyName("sport_type")]
    public string SportType { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("private")]
    public bool Private { get; set; }

    [JsonPropertyName("member_count")]
    public int MemberCount { get; set; }

    [JsonPropertyName("featured")]
    public bool Featured { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("url")]
    public bool Url { get; set; }
}

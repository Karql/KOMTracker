using Strava.API.Client.Model.Club;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Club;
public static class ClubSummaryModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this ClubSummaryModel model)
    {
        return @$"{{
            ""id"": {model.Id},
            ""resource_state"": {(int)model.ResourceState},
            ""name"": ""{model.Name}"",
            ""profile_medium"": ""{model.ProfileMedium}"",
            ""profile"": ""{model.Profile}"",
            ""cover_photo"": ""{model.CoverPhoto}"",
            ""cover_photo_small"": ""{model.CoverPhotoSmall}"",
            ""activity_types"": [{string.Join(", ", model.ActivityTypes.Select(x => '"' + x + '"'))}],
            ""activity_types_icon"": ""{model.ActivityTypesIcon}"",
            ""dimensions"": [{string.Join(", ", model.Dimensions.Select(x => '"' + x + '"'))}],
            ""sport_type"": ""{model.SportType}"",
            ""city"": ""{model.City}"",
            ""state"": ""{model.State}"",
            ""country"": ""{model.Country}"",
            ""private"": {model.Private.ToLowerString()},
            ""member_count"": {model.MemberCount},
            ""featured"": {model.Featured.ToLowerString()},
            ""verified"": {model.Verified.ToLowerString()},
            ""url"": ""{model.Url}""
        }}";
    }

    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this IEnumerable<ClubSummaryModel> list)
    {
        return @$"[
                {string.Join(", ", list.Select(x => x.ToJson()))}
            ]";
    }
}

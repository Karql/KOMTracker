using Strava.API.Client.Model.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Athlete;

public static class AthleteSummaryModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this AthleteSummaryModel model)
    {
        return @$"{{
                ""id"": {model.Id},
                ""username"": ""{model.Username}"",
                ""resource_state"": {(int)model.ResourceState},
                ""firstname"": ""{model.FirstName}"",
                ""lastname"": ""{model.Lastname}"",
                ""bio"": ""{model.Bio}"",
                ""city"": ""{model.City}"",
                ""state"": ""{model.State}"",
                ""country"": ""{model.Country}"",
                ""sex"": ""{model.Sex}"",
                ""premium"": {model.Premium.ToLowerString()},
                ""summit"": {model.Summit.ToLowerString()},
                ""created_at"": ""{model.CreatedAt.ToUtcIso()}"",
                ""updated_at"": ""{model.UpdatedAt.ToUtcIso()}"",
                ""badge_type_id"": {model.BadgeTypeId},
                ""weight"": {model.Weight},
                ""profile_medium"": ""{model.ProfileMedium}"",
                ""profile"": ""{model.Profile}""
            }}";
    }
}

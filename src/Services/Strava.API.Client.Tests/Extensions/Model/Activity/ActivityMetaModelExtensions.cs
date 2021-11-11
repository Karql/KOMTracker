using Strava.API.Client.Model.Activity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Activity;

public static class ActivityMetaModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this ActivityMetaModel model)
    {
        return @$"{{
                ""resource_state"": {(int)model.ResourceState},
                ""id"": {model.Id}              
            }}";
    }
}

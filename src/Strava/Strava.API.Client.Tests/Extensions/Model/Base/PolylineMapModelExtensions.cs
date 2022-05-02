using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Tests.Extensions.Model.Base;
public static class PolylineMapModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this PolylineMapModel model)
    {
        return @$"{{
            ""polyline"": ""{model.Polyline}""
        }}";
    }
}

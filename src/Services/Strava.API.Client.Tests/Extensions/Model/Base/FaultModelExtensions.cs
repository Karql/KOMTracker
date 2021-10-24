using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Tests.Extensions.Model.Base
{
    public static class FaultModelExtensions
    {
        public static string ToJson(this FaultModel model)
        {
            /// <summary>
            /// JSON equivalent to API response
            /// </summary>
            /// <remarks>
            /// Manually created string string for testing deserialization
            /// </remarks>
            return @$"{{
                ""message"": ""{model.Message}"",
                ""errors"": [
                    {string.Join(", ", model.Errors.Select(x => x.ToJson()))}
                ]
            }}";
        }
    }
}

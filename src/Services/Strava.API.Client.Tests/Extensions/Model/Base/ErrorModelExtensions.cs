using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Tests.Extensions.Model.Base
{
    public static class ErrorModelExtensions
    {
        public static string ToJson(this ErrorModel model)
        {
            /// <summary>
            /// JSON equivalent to API response
            /// </summary>
            /// <remarks>
            /// Manually created string string for testing deserialization
            /// </remarks>
            return @$"{{
                ""resource"": ""{model.Resource}"",
                ""field"": ""{model.Field}"",
                ""code"": ""{model.Code}""
            }}";
        }
    }
}

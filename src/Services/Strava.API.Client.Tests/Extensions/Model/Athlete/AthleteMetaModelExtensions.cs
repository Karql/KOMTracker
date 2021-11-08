using Strava.API.Client.Model.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Athlete
{
    public static class AthleteMetaModelExtensions
    {
        /// <summary>
        /// JSON equivalent to API response
        /// </summary>
        /// <remarks>
        /// Manually created string string for testing deserialization
        /// </remarks>
        public static string ToJson(this AthleteMetaModel model)
        {
            return @$"{{
                ""resource_state"": {(int)model.ResourceState},
                ""id"": {model.Id}              
            }}";
        }
    }
}

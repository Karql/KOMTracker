using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Base
{
    public class ErrorModel
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }

        [JsonPropertyName("field")]
        public string Field { get; set; }

        [JsonPropertyName("resource")]
        public string Resource { get; set; }
    }
}

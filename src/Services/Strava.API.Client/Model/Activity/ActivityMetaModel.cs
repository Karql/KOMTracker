using Strava.API.Client.Model.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Activity
{
    public class ActivityMetaModel
    {
        [JsonPropertyName("resource_state")]
        public ResourceStateEnum ResourceState { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }
    }
}

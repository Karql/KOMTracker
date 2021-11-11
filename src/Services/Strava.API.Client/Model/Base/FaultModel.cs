using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Base;

public class FaultModel
{
    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("errors")]
    public ErrorModel[] Errors { get; set; }
}

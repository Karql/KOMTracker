using Strava.API.Client.Model.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Token
{
    public class TokenWithAthleteModel : TokenModel
    {
        [JsonPropertyName("athlete")]
        public AthleteModel Athlete { get; set; }
    }
}

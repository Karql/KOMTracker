using Strava.API.Client.Model.Athlete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Strava.API.Client.Model.Token;

public class TokenWithAthleteModel : TokenModel
{
    public TokenWithAthleteModel() { }

    public TokenWithAthleteModel(TokenModel token, AthleteSummaryModel athlete)
    {
        TokenType = token.TokenType;
        ExpiresAt = token.ExpiresAt;
        ExpiresIn = token.ExpiresIn;
        RefreshToken = token.RefreshToken;
        AccessToken = token.AccessToken;
        Athlete = athlete;
    }

    [JsonPropertyName("athlete")]
    public AthleteSummaryModel Athlete { get; set; }
}

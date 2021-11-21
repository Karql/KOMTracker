using Strava.API.Client.Model.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace Strava.API.Client.Tests.Extensions.Model.Athlete;

public static class TokenModelExtensions
{
    /// <summary>
    /// JSON equivalent to API response
    /// </summary>
    /// <remarks>
    /// Manually created string string for testing deserialization
    /// </remarks>
    public static string ToJson(this TokenModel model)
    {
        return @$"{{
                ""token_type"": ""{model.TokenType}"",
                ""expires_at"": {model.ExpiresAt.ToTimeStamp()},
                ""expires_in"": {model.ExpiresIn},
                ""refresh_token"": ""{model.RefreshToken}"",
                ""access_token"": ""{model.AccessToken}""
            }}";
    }
}

using FluentResults;
using Microsoft.Extensions.Logging;
using Strava.API.Client.Model.Base;
using Strava.API.Client.Model.Config;
using Strava.API.Client.Model.Token;
using Strava.API.Client.Model.Token.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Strava.API.Client.Api
{
    public class TokenApi : ITokenApi
    {
        private readonly ILogger<TokenApi> _logger;
        private readonly ConfigModel _config;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenApi(ILogger<TokenApi> logger, ConfigModel config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Result<TokenWithAthleteModel>> ExchangeAsync(string code)
        {            
            var url = $"https://www.strava.com/oauth/token?client_id={_config.ClientID}&client_secret={_config.ClientSecret}&code={code}&grant_type=authorization_code";

            var logPrefix = $"{nameof(ExchangeAsync)} ";
            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.PostAsync(url, null);

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadFromJsonAsync<TokenWithAthleteModel>();
                return Result.Ok(token);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var fault = await response.Content.ReadFromJsonAsync<FaultModel>();
                
                if (fault?.Errors?.Any() ?? false)
                {
                    var error = fault.Errors.First();

                    if (error.Code == "invalid" && error.Field == "code" && error.Resource == "AuthorizationCode")
                    {
                        _logger.LogWarning(logPrefix + "Invalid code!");
                        return Result.Fail(new ExchangeError(ExchangeError.InvalidCode));
                    }
                }
            }

            _logger.LogError(logPrefix + "failed! SatusCode: {statusCode}, Response: {response}", 
                (int)response.StatusCode, await response.Content.ReadAsStringAsync());

            return Result.Fail(new ExchangeError(ExchangeError.UnknownError));
        }
    }
}

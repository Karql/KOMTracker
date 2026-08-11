using FluentResults;
using Microsoft.Extensions.Logging;
using Strava.API.Client.Model.Gear;
using Strava.API.Client.Model.Gear.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public class GearApi : IGearApi
{
    private readonly ILogger<GearApi> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GearApi(ILogger<GearApi> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<Result<GearDetailedModel>> GetGearAsync(string gearId, string token)
    {
        var url = $"https://www.strava.com/api/v3/gear/{gearId}";

        var logPrefix = $"{nameof(GetGearAsync)} ";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var gear = await response.Content.ReadFromJsonAsync<GearDetailedModel>();
            return Result.Ok(gear);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(logPrefix + "Unauthorized! Gear Id: {gearId}", gearId);
            return Result.Fail<GearDetailedModel>(new GetGearError(GetGearError.Unauthorized));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            IEnumerable<string> values;
            var rateLimitLimit = response.Headers.TryGetValues("X-RateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var rateLimitUsage = response.Headers.TryGetValues("X-RateLimit-Usage", out values) ? values.FirstOrDefault() : null;
            var readRateLimitLimit = response.Headers.TryGetValues("X-ReadRateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var readRateLimitUsage = response.Headers.TryGetValues("X-ReadRateLimit-Usage", out values) ? values.FirstOrDefault() : null;

            _logger.LogError(logPrefix + "Rate Limit Exceeded! Gear Id: {gearId}, X-RateLimit-Limit: {rateLimitLimit}, X-RateLimit-Usage: {rateLimitUsage}, X-ReadRateLimit-Limit: {readRateLimitLimit}, X-ReadRateLimit-Usage: {readRateLimitUsage}",
                gearId, rateLimitLimit, rateLimitUsage, readRateLimitLimit, readRateLimitUsage);
            return Result.Fail<GearDetailedModel>(new GetGearError(GetGearError.TooManyRequests));
        }

        _logger.LogError(logPrefix + "failed! Gear Id: {gearId}, SatusCode: {statusCode}, Response: {response}",
            gearId, (int)response.StatusCode, await response.Content.ReadAsStringAsync());

        return Result.Fail<GearDetailedModel>(new GetGearError(GetGearError.UnknownError));
    }
}

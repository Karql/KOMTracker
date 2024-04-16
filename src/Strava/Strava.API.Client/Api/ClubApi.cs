using FluentResults;
using Microsoft.Extensions.Logging;
using Strava.API.Client.Model.Club;
using Strava.API.Client.Model.Club.Error;
using Strava.API.Client.Model.Segment.Error;
using Strava.API.Client.Model.Segment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;
public class ClubApi : IClubApi
{
    private const int MAX_PER_PAGE = 200;

    private readonly ILogger<ClubApi> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ClubApi(ILogger<ClubApi> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<Result<IEnumerable<ClubSummaryModel>>> GetClubsAsync(int athleteId, string token)
    {
        var clubs = new List<ClubSummaryModel>();
        int page = 1;

        while (true)
        {
            var res = await GetClubsAsync(athleteId, token, page++);

            if (res.IsFailed)
            {
                return res;
            }

            if (!(res.Value?.Any() ?? false))
            {
                return Result.Ok(clubs.AsEnumerable());
            }

            clubs.AddRange(res.Value);
        }
    }

    private async Task<Result<IEnumerable<ClubSummaryModel>>> GetClubsAsync(int athleteId, string token, int page)
    {
        var url = $"https://www.strava.com/api/v3/athlete/clubs?per_page={MAX_PER_PAGE}&page={page}";

        var logPrefix = $"{nameof(GetClubsAsync)} ";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var clubs = await response.Content.ReadFromJsonAsync<List<ClubSummaryModel>>();
            return Result.Ok(clubs.AsEnumerable());
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(logPrefix + "Unauthorized! Athlete Id: {athleteId}, Page: {page}", athleteId, page);
            return Result.Fail<IEnumerable<ClubSummaryModel>>(new GetClubsError(GetClubsError.Unauthorized));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            IEnumerable<string> values;
            var rateLimitLimit = response.Headers.TryGetValues("X-RateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var rateLimitUsage = response.Headers.TryGetValues("X-RateLimit-Usage", out values) ? values.FirstOrDefault() : null;
            var readRateLimitLimit = response.Headers.TryGetValues("X-ReadRateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var readRateLimitUsage = response.Headers.TryGetValues("X-ReadRateLimit-Usage", out values) ? values.FirstOrDefault() : null;

            _logger.LogError(logPrefix + "Rate Limit Exceeded! Athlete Id: {athleteId}, Page: {page}, X-RateLimit-Limit: {rateLimitLimit}, X-RateLimit-Usage: {rateLimitUsage}, X-ReadRateLimit-Limit: {readRateLimitLimit}, X-ReadRateLimit-Usage: {readRateLimitUsage}",
                athleteId, page, rateLimitLimit, rateLimitUsage, readRateLimitLimit, readRateLimitUsage);
            return Result.Fail<IEnumerable<ClubSummaryModel>>(new GetClubsError(GetClubsError.TooManyRequests));
        }

        _logger.LogError(logPrefix + "failed! Athlete Id: {athleteId}, Page: {page}, SatusCode: {statusCode}, Response: {response}",
            athleteId, page, (int)response.StatusCode, await response.Content.ReadAsStringAsync());

        return Result.Fail<IEnumerable<ClubSummaryModel>>(new GetClubsError(GetClubsError.UnknownError));
    }
}

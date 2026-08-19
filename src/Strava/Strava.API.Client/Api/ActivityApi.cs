using FluentResults;
using Microsoft.Extensions.Logging;
using Strava.API.Client.Model.Activity;
using Strava.API.Client.Model.Activity.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public class ActivityApi : IActivityApi
{
    private const int MAX_PER_PAGE = 200;

    private readonly ILogger<ActivityApi> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ActivityApi(ILogger<ActivityApi> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<Result<IEnumerable<ActivitySummaryModel>>> GetActivitiesAsync(string token, long? after = null, long? before = null)
    {
        var activities = new List<ActivitySummaryModel>();
        int page = 1;

        // Iterate until an empty page — same convention as AthleteApi.GetKomsAsync.
        while (true)
        {
            var res = await GetActivitiesAsync(token, page++, after, before);

            if (res.IsFailed)
            {
                return res;
            }

            if (!(res.Value?.Any() ?? false))
            {
                return Result.Ok(activities.AsEnumerable());
            }

            activities.AddRange(res.Value);
        }
    }

    private async Task<Result<IEnumerable<ActivitySummaryModel>>> GetActivitiesAsync(string token, int page, long? after, long? before)
    {
        var url = $"https://www.strava.com/api/v3/athlete/activities?per_page={MAX_PER_PAGE}&page={page}";

        if (after.HasValue)
        {
            url += $"&after={after.Value}";
        }

        if (before.HasValue)
        {
            url += $"&before={before.Value}";
        }

        var logPrefix = $"{nameof(GetActivitiesAsync)} ";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var activities = await response.Content.ReadFromJsonAsync<List<ActivitySummaryModel>>();
            return Result.Ok(activities.AsEnumerable());
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(logPrefix + "Unauthorized! Page: {page}", page);
            return Result.Fail<IEnumerable<ActivitySummaryModel>>(new GetActivitiesError(GetActivitiesError.Unauthorized));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            IEnumerable<string> values;
            var rateLimitLimit = response.Headers.TryGetValues("X-RateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var rateLimitUsage = response.Headers.TryGetValues("X-RateLimit-Usage", out values) ? values.FirstOrDefault() : null;
            var readRateLimitLimit = response.Headers.TryGetValues("X-ReadRateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var readRateLimitUsage = response.Headers.TryGetValues("X-ReadRateLimit-Usage", out values) ? values.FirstOrDefault() : null;

            _logger.LogError(logPrefix + "Rate Limit Exceeded! Page: {page}, X-RateLimit-Limit: {rateLimitLimit}, X-RateLimit-Usage: {rateLimitUsage}, X-ReadRateLimit-Limit: {readRateLimitLimit}, X-ReadRateLimit-Usage: {readRateLimitUsage}",
                page, rateLimitLimit, rateLimitUsage, readRateLimitLimit, readRateLimitUsage);
            return Result.Fail<IEnumerable<ActivitySummaryModel>>(new GetActivitiesError(GetActivitiesError.TooManyRequests));
        }

        _logger.LogError(logPrefix + "failed! Page: {page}, SatusCode: {statusCode}, Response: {response}",
            page, (int)response.StatusCode, await response.Content.ReadAsStringAsync());

        return Result.Fail<IEnumerable<ActivitySummaryModel>>(new GetActivitiesError(GetActivitiesError.UnknownError));
    }

    public async Task<Result<ActivityDetailedModel>> GetActivityAsync(long activityId, string token)
    {
        var url = $"https://www.strava.com/api/v3/activities/{activityId}";

        var logPrefix = $"{nameof(GetActivityAsync)} ";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var activity = await response.Content.ReadFromJsonAsync<ActivityDetailedModel>();
            return Result.Ok(activity);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(logPrefix + "Unauthorized! Activity Id: {activityId}", activityId);
            return Result.Fail<ActivityDetailedModel>(new GetActivityError(GetActivityError.Unauthorized));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            IEnumerable<string> values;
            var rateLimitLimit = response.Headers.TryGetValues("X-RateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var rateLimitUsage = response.Headers.TryGetValues("X-RateLimit-Usage", out values) ? values.FirstOrDefault() : null;
            var readRateLimitLimit = response.Headers.TryGetValues("X-ReadRateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var readRateLimitUsage = response.Headers.TryGetValues("X-ReadRateLimit-Usage", out values) ? values.FirstOrDefault() : null;

            _logger.LogError(logPrefix + "Rate Limit Exceeded! Activity Id: {activityId}, X-RateLimit-Limit: {rateLimitLimit}, X-RateLimit-Usage: {rateLimitUsage}, X-ReadRateLimit-Limit: {readRateLimitLimit}, X-ReadRateLimit-Usage: {readRateLimitUsage}",
                activityId, rateLimitLimit, rateLimitUsage, readRateLimitLimit, readRateLimitUsage);
            return Result.Fail<ActivityDetailedModel>(new GetActivityError(GetActivityError.TooManyRequests));
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(logPrefix + "Not Found! Activity Id: {activityId}", activityId);
            return Result.Fail<ActivityDetailedModel>(new GetActivityError(GetActivityError.NotFound));
        }

        _logger.LogError(logPrefix + "failed! Activity Id: {activityId}, SatusCode: {statusCode}, Response: {response}",
            activityId, (int)response.StatusCode, await response.Content.ReadAsStringAsync());

        return Result.Fail<ActivityDetailedModel>(new GetActivityError(GetActivityError.UnknownError));
    }
}

using FluentResults;
using Microsoft.Extensions.Logging;
using Strava.API.Client.Model.Segment;
using Strava.API.Client.Model.Segment.Error;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public class SegmentApi : ISegmentApi
{
    private readonly ILogger<SegmentApi> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public SegmentApi(ILogger<SegmentApi> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public async Task<Result<SegmentDetailedModel>> GetSegmentAsync(long id, string token)
    {
        var url = $"https://www.strava.com/api/v3/segments/{id}";

        var logPrefix = $"{nameof(GetSegmentAsync)} ";
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await httpClient.GetAsync(url);

        if (response.IsSuccessStatusCode)
        {
            var segment = await response.Content.ReadFromJsonAsync<SegmentDetailedModel>();
            return Result.Ok(segment);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(logPrefix + "Unauthorized! Segment id: {segmentId}", id);
            return Result.Fail<SegmentDetailedModel>(new GetSegmentError(GetSegmentError.Unauthorized));
        }

        else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning(logPrefix + "Not found! Segment id: {segmentId}", id);
            return Result.Fail<SegmentDetailedModel>(new GetSegmentError(GetSegmentError.NotFound));
        }

        else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            IEnumerable<string> values;
            var rateLimitLimit = response.Headers.TryGetValues("X-RateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var rateLimitUsage = response.Headers.TryGetValues("X-RateLimit-Usage", out values) ? values.FirstOrDefault() : null;
            var readRateLimitLimit = response.Headers.TryGetValues("X-ReadRateLimit-Limit", out values) ? values.FirstOrDefault() : null;
            var readRateLimitUsage = response.Headers.TryGetValues("X-ReadRateLimit-Usage", out values) ? values.FirstOrDefault() : null;

            _logger.LogError(logPrefix + "Rate Limit Exceeded! X-RateLimit-Limit: {rateLimitLimit}, X-RateLimit-Usage: {rateLimitUsage}, X-ReadRateLimit-Limit: {readRateLimitLimit}, X-ReadRateLimit-Usage: {readRateLimitUsage}, Segment id: {segmentId}",
                rateLimitLimit, rateLimitUsage, readRateLimitLimit, readRateLimitUsage, id);

            return Result.Fail<SegmentDetailedModel>(new GetSegmentError(GetSegmentError.TooManyRequests));
        }

        _logger.LogError(logPrefix + "failed! SatusCode: {statusCode}, Response: {response}, Segment id: {segmentId}",
            (int)response.StatusCode, await response.Content.ReadAsStringAsync(), id);

        return Result.Fail<SegmentDetailedModel>(new GetSegmentError(GetSegmentError.UnknownError));
    }
}

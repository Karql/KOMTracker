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

namespace Strava.API.Client.Api
{
    public class AthleteApi : IAthleteApi
    {
        private const int MAX_PER_PAGE = 200;

        private readonly ILogger<AthleteApi> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AthleteApi(ILogger<AthleteApi> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<Result<IEnumerable<SegmentEffortDetailedModel>>> GetKomsAsync(string token)
        {
            var koms = new List<SegmentEffortDetailedModel>();
            int page = 1;

            while (true)
            {
                var res = await GetKomsAsync(token, page++);

                if (res.IsFailed)
                {
                    return res;
                }

                if (!(res.Value?.Any() ?? false))
                {
                    return Result.Ok(koms.AsEnumerable());
                }

                koms.AddRange(res.Value);
            }
        }

        private async Task<Result<IEnumerable<SegmentEffortDetailedModel>>> GetKomsAsync(string token, int page)
        {
            var url = $"https://www.strava.com/api/v3/athletes/2394302/koms?per_page={MAX_PER_PAGE}&page={page}";

            var logPrefix = $"{nameof(GetKomsAsync)} ";
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            using var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var koms = await response.Content.ReadFromJsonAsync<List<SegmentEffortDetailedModel>>();
                return Result.Ok(koms.AsEnumerable());
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning(logPrefix + "Unauthorized!");
                return Result.Fail<IEnumerable<SegmentEffortDetailedModel>>(new GetKomsError(GetKomsError.Unauthorized));
            }

            _logger.LogError(logPrefix + "failed! SatusCode: {statusCode}, Response: {response}",
                (int)response.StatusCode, await response.Content.ReadAsStringAsync());

            return Result.Fail<IEnumerable<SegmentEffortDetailedModel>>(new GetKomsError(GetKomsError.UnknownError));
        }
    }
}

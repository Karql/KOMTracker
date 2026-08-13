#nullable enable
using FluentResults;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Strava.Mappings;
using Microsoft.Extensions.Logging;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiBase = Strava.API.Client.Model.Base.Error;
using ApiGear = Strava.API.Client.Model.Gear;

namespace KomTracker.Infrastructure.Strava.Services;

public class GearService : IGearService
{
    private readonly IAthleteApi _athleteApi;
    private readonly IGearApi _gearApi;
    private readonly ILogger<GearService> _logger;

    public GearService(IAthleteApi athleteApi, IGearApi gearApi, ILogger<GearService> logger)
    {
        _athleteApi = athleteApi ?? throw new ArgumentNullException(nameof(athleteApi));
        _gearApi = gearApi ?? throw new ArgumentNullException(nameof(gearApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<StravaBikeEntity>>> GetAthleteBikesAsync(int athleteId, string token)
    {
        var athleteRes = await _athleteApi.GetAthleteAsync(token);
        if (athleteRes.IsFailed)
        {
            return Result.Fail<IEnumerable<StravaBikeEntity>>(new GetAthleteBikesError(MapError(athleteRes.Errors)));
        }

        var summaries = athleteRes.Value.Bikes ?? Array.Empty<ApiGear.GearSummaryModel>();
        var bikes = new List<StravaBikeEntity>(summaries.Length);

        foreach (var summary in summaries)
        {
            var gearRes = await _gearApi.GetGearAsync(summary.Id, token);
            if (gearRes.IsSuccess)
            {
                bikes.Add(gearRes.Value.ToStravaBikeEntity(athleteId));
                continue;
            }

            var message = MapError(gearRes.Errors);
            // Rate limit / auth issues are terminal — surface and stop.
            if (message is GetAthleteBikesError.TooManyRequests or GetAthleteBikesError.Unauthorized)
            {
                return Result.Fail<IEnumerable<StravaBikeEntity>>(new GetAthleteBikesError(message));
            }

            // Otherwise keep the bike from the summary (still store it, just without detailed fields).
            _logger.LogWarning("{service}: gear {gearId} detail unavailable ({error}) - storing summary only",
                nameof(GearService), summary.Id, message);
            bikes.Add(summary.ToStravaBikeEntity(athleteId));
        }

        return Result.Ok(bikes.AsEnumerable());
    }

    private static string MapError(IReadOnlyList<IError> errors) =>
        errors.OfType<ApiBase.BaseError>().FirstOrDefault()?.Message switch
        {
            ApiBase.BaseError.Unauthorized => GetAthleteBikesError.Unauthorized,
            ApiBase.BaseError.TooManyRequests => GetAthleteBikesError.TooManyRequests,
            _ => GetAthleteBikesError.UnknownError
        };
}

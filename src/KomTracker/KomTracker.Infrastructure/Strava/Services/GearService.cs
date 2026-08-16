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

    public async Task<Result<IEnumerable<StravaBikeEntity>>> GetAthleteBikesAsync(int athleteId, string token, IReadOnlyCollection<string> extraGearIds)
    {
        var athleteRes = await _athleteApi.GetAthleteAsync(token);
        if (athleteRes.IsFailed)
        {
            return Result.Fail<IEnumerable<StravaBikeEntity>>(new GetAthleteBikesError(MapError(athleteRes.Errors)));
        }

        // Athlete bikes[] gives only ACTIVE gear (Strava omits retired). Union with extra ids (e.g. bike
        // gear ids seen in activities) so retired/historical bikes get hydrated + imported too.
        var summariesById = (athleteRes.Value.Bikes ?? Array.Empty<ApiGear.GearSummaryModel>())
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.First());

        var gearIds = summariesById.Keys
            .Concat(extraGearIds ?? Array.Empty<string>())
            .Distinct()
            .ToList();

        var bikes = new List<StravaBikeEntity>(gearIds.Count);

        foreach (var gearId in gearIds)
        {
            var gearRes = await _gearApi.GetGearAsync(gearId, token);
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

            // Detail unavailable: fall back to the summary if we have one (active bikes); an activity-derived
            // id has no summary, so just skip it.
            if (summariesById.TryGetValue(gearId, out var summary))
            {
                _logger.LogWarning("{service}: gear {gearId} detail unavailable ({error}) - storing summary only",
                    nameof(GearService), gearId, message);
                bikes.Add(summary.ToStravaBikeEntity(athleteId));
            }
            else
            {
                _logger.LogWarning("{service}: gear {gearId} detail unavailable ({error}) and no summary - skipping",
                    nameof(GearService), gearId, message);
            }
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

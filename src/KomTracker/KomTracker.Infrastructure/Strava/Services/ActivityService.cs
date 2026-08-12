using FluentResults;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Domain.Entities.Strava;
using KomTracker.Infrastructure.Strava.Mappings;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Strava.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityApi _activityApi;

    public ActivityService(IActivityApi activityApi)
    {
        _activityApi = activityApi ?? throw new ArgumentNullException(nameof(activityApi));
    }

    public async Task<Result<IEnumerable<ActivityEntity>>> GetAthleteActivitiesAsync(int athleteId, string token, long? after)
    {
        var getActivitiesRes = await _activityApi.GetActivitiesAsync(token, after);

        if (getActivitiesRes.IsSuccess)
        {
            return Result.Ok(getActivitiesRes.Value
                .Select(x => x.ToEntity(athleteId))
                .ToList()
                .AsEnumerable());
        }

        var mappedErrorMessage = getActivitiesRes.Errors.OfType<ApiModel.Activity.Error.GetActivitiesError>().FirstOrDefault()?.Message switch
        {
            ApiModel.Activity.Error.GetActivitiesError.Unauthorized => GetAthleteActivitiesError.Unauthorized,
            ApiModel.Activity.Error.GetActivitiesError.TooManyRequests => GetAthleteActivitiesError.TooManyRequests,
            _ => GetAthleteActivitiesError.UnknownError
        };

        return Result.Fail<IEnumerable<ActivityEntity>>(new GetAthleteActivitiesError(mappedErrorMessage));
    }
}

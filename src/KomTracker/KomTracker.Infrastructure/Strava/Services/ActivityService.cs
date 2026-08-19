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

    public async Task<Result<ActivityEntity>> GetAthleteActivityAsync(int athleteId, string token, long activityId)
    {
        var getActivityRes = await _activityApi.GetActivityAsync(activityId, token);

        if (getActivityRes.IsSuccess)
        {
            // Guard: only persist the activity when it actually belongs to the requesting athlete — the id comes
            // from the client, so without this a caller could pull another athlete's public ride under their own id.
            if (getActivityRes.Value.Athlete?.Id != athleteId)
            {
                return Result.Fail<ActivityEntity>(new GetAthleteActivitiesError(GetAthleteActivitiesError.NotFound));
            }

            return Result.Ok(getActivityRes.Value.ToEntity(athleteId));
        }

        var mappedErrorMessage = getActivityRes.Errors.OfType<ApiModel.Activity.Error.GetActivityError>().FirstOrDefault()?.Message switch
        {
            ApiModel.Activity.Error.GetActivityError.Unauthorized => GetAthleteActivitiesError.Unauthorized,
            ApiModel.Activity.Error.GetActivityError.TooManyRequests => GetAthleteActivitiesError.TooManyRequests,
            ApiModel.Activity.Error.GetActivityError.NotFound => GetAthleteActivitiesError.NotFound,
            _ => GetAthleteActivitiesError.UnknownError
        };

        return Result.Fail<ActivityEntity>(new GetAthleteActivitiesError(mappedErrorMessage));
    }
}

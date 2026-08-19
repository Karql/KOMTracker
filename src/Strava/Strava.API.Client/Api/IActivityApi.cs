using FluentResults;
using Strava.API.Client.Model.Activity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Strava.API.Client.Api;

public interface IActivityApi
{
    /// <summary>
    /// Lists the authenticated athlete's activities (GET /athlete/activities), paging through all results.
    /// <paramref name="after"/>/<paramref name="before"/> are epoch seconds (inclusive/exclusive window).
    /// </summary>
    Task<Result<IEnumerable<ActivitySummaryModel>>> GetActivitiesAsync(string token, long? after = null, long? before = null);

    /// <summary>Fetch a single activity (GET /activities/{id}) as a full DetailedActivity.</summary>
    Task<Result<ActivityDetailedModel>> GetActivityAsync(long activityId, string token);
}

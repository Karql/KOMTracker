using FluentResults;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface IActivityService
{
    /// <summary>Fetch an athlete's Strava activities (optionally after an epoch-seconds instant), mapped to entities.</summary>
    Task<Result<IEnumerable<ActivityEntity>>> GetAthleteActivitiesAsync(int athleteId, string token, long? after);

    /// <summary>
    /// Fetch a single Strava activity (GET /activities/{id}) mapped to an entity. Fails with
    /// <see cref="GetAthleteActivitiesError.NotFound"/> if the activity doesn't exist or doesn't belong to the athlete.
    /// </summary>
    Task<Result<ActivityEntity>> GetAthleteActivityAsync(int athleteId, string token, long activityId);
}

public class GetAthleteActivitiesError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string TooManyRequests = "TooManyRequests";
    public const string NotFound = "NotFound";
    public const string UnknownError = "UnknownError";

    public GetAthleteActivitiesError(string message)
        : base(message)
    {
    }
}

using FluentResults;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface IActivityService
{
    /// <summary>Fetch an athlete's Strava activities (optionally after an epoch-seconds instant), mapped to entities.</summary>
    Task<Result<IEnumerable<ActivityEntity>>> GetAthleteActivitiesAsync(int athleteId, string token, long? after);
}

public class GetAthleteActivitiesError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string TooManyRequests = "TooManyRequests";
    public const string UnknownError = "UnknownError";

    public GetAthleteActivitiesError(string message)
        : base(message)
    {
    }
}

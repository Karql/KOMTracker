using FluentResults;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface IGearService
{
    /// <summary>
    /// Fetch the athlete's Strava gear (bikes[], incl. retired) hydrated with DetailedGear
    /// (brand/model/frame_type/weight), mapped to entities.
    /// </summary>
    Task<Result<IEnumerable<StravaBikeEntity>>> GetAthleteBikesAsync(int athleteId, string token);
}

public class GetAthleteBikesError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string TooManyRequests = "TooManyRequests";
    public const string UnknownError = "UnknownError";

    public GetAthleteBikesError(string message)
        : base(message)
    {
    }
}

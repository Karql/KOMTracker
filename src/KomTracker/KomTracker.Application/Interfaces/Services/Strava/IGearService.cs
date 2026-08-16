using FluentResults;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface IGearService
{
    /// <summary>
    /// Fetch the athlete's Strava bikes hydrated with DetailedGear (brand/model/frame_type/weight),
    /// mapped to entities. Fetches the union of the athlete's current bikes[] and <paramref name="extraGearIds"/>
    /// (e.g. bike gear ids seen in activities) so retired/historical bikes — absent from GET /athlete — are imported too.
    /// </summary>
    Task<Result<IEnumerable<StravaBikeEntity>>> GetAthleteBikesAsync(int athleteId, string token, IReadOnlyCollection<string> extraGearIds);
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

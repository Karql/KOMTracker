namespace KomTracker.Application.Errors.Strava.Athlete;

public class GetAthleteKomsError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string UnknownError = "UnknownError";

    public GetAthleteKomsError(string message)
        : base(message)
    {
    }
}

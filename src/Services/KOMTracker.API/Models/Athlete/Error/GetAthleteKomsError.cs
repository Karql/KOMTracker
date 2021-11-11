namespace KOMTracker.API.Models.Athlete.Error;

public class GetAthleteKomsError : FluentResults.Error
{
    public const string Unauthorized = "Unauthorized";
    public const string UnknownError = "UnknownError";

    public GetAthleteKomsError(string message)
        : base(message)
    {
    }
}

namespace KOMTracker.API.Models.Athlete.Error;

public class GetValidTokenError : FluentResults.Error
{
    public const string NoTokenInDB = "No token in DB!";
    public const string RefreshFailed = "Refresh failed!";

    public GetValidTokenError(string message)
        : base(message)
    {
    }
}

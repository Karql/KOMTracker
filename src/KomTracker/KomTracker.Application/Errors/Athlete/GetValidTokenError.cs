namespace KomTracker.Domain.Errors.Athlete;

public class GetValidTokenError : FluentResults.Error
{
    public const string NoTokenInDB = "No token in DB!";
    public const string RefreshFailed = "Refresh failed!";

    public GetValidTokenError(string message)
        : base(message)
    {
    }
}

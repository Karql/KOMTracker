namespace KomTracker.Application.Errors.Strava.Token;

public class RefreshError : FluentResults.Error
{
    public const string InvalidRefreshToken = "Invalid refresh token";
    public const string UnknownError = "Unknown error";

    public RefreshError(string message)
        : base(message)
    {
    }
}

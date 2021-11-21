namespace KomTracker.Application.Errors.Strava.Token;

public class ExchangeError : FluentResults.Error
{
    public const string InvalidCode = "Invalid code";
    public const string UnknownError = "Unknown error";

    public ExchangeError(string message)
        : base(message)
    {
    }
}

namespace KomTracker.Application.Errors;

/// <summary>Requested resource does not exist. Maps to HTTP 404.</summary>
public class NotFoundError : AppError
{
    public NotFoundError(string message)
        : base(message)
    {
    }
}

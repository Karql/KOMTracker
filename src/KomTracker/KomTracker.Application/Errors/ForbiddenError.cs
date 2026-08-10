namespace KomTracker.Application.Errors;

/// <summary>Caller is not allowed to access/modify the resource. Maps to HTTP 403.</summary>
public class ForbiddenError : AppError
{
    public ForbiddenError(string message)
        : base(message)
    {
    }
}

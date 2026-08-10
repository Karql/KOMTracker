namespace KomTracker.Application.Errors;

/// <summary>Request conflicts with current state. Maps to HTTP 409. (Defined for future use.)</summary>
public class ConflictError : AppError
{
    public ConflictError(string message)
        : base(message)
    {
    }
}

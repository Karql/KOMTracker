using FluentResults;

namespace KomTracker.Application.Errors;

/// <summary>
/// Base for semantic application errors that the API maps to an HTTP status
/// (see the Result -> IActionResult mapping). Errors that are not an
/// <see cref="AppError"/> map to 400 by default.
/// </summary>
public abstract class AppError : Error
{
    protected AppError(string message)
        : base(message)
    {
    }
}

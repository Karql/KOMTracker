using FluentResults;
using KomTracker.Application.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace KomTracker.API.Extensions;

/// <summary>
/// Maps a FluentResults <see cref="Result"/> to an <see cref="IActionResult"/> by switching on the
/// first error's semantic type: ValidationError→422, NotFoundError→404, ForbiddenError→403,
/// ConflictError→409, anything else→400. Bodies are built with the framework's
/// <see cref="ProblemDetailsFactory"/> (RFC 7807: type/title/status/traceId) and returned with the
/// correct HTTP status (unlike <c>ValidationProblem()</c>, which is hardwired to 400).
/// </summary>
public static class ResultExtensions
{
    public static IActionResult ToActionResult(this ControllerBase controller, Result result)
        => result.IsSuccess ? controller.NoContent() : controller.Failure(result.Errors);

    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result, Func<T, object?> okValueSelector)
        => result.IsSuccess ? controller.Ok(okValueSelector(result.Value)) : controller.Failure(result.Errors);

    private static IActionResult Failure(this ControllerBase controller, IReadOnlyList<IError> errors)
    {
        var first = errors.FirstOrDefault();

        switch (first)
        {
            case ValidationError ve:
                var modelState = new ModelStateDictionary();
                foreach (var (field, messages) in ve.Errors)
                {
                    foreach (var message in messages)
                    {
                        modelState.AddModelError(field, message);
                    }
                }

                var validationProblem = controller.ProblemDetailsFactory.CreateValidationProblemDetails(
                    controller.HttpContext, modelState, statusCode: StatusCodes.Status422UnprocessableEntity);
                return Problem(validationProblem);

            case NotFoundError:
                return controller.SemanticProblem(StatusCodes.Status404NotFound, first.Message);

            case ForbiddenError:
                return controller.SemanticProblem(StatusCodes.Status403Forbidden, first.Message);

            case ConflictError:
                return controller.SemanticProblem(StatusCodes.Status409Conflict, first.Message);

            default:
                return controller.SemanticProblem(
                    StatusCodes.Status400BadRequest,
                    string.Join(" ", errors.Select(e => e.Message)));
        }
    }

    private static IActionResult SemanticProblem(this ControllerBase controller, int statusCode, string detail)
    {
        var problemDetails = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext, statusCode: statusCode, detail: detail);
        return Problem(problemDetails);
    }

    // Content-Type is set to application/problem+json automatically by the output formatter
    // whenever the ObjectResult value derives from ProblemDetails — no need to set it here.
    private static IActionResult Problem(ProblemDetails problemDetails)
        => new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
}

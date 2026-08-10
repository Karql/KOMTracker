using FluentResults;
using FluentValidation;
using KomTracker.Application.Errors;
using MediatR;

namespace KomTracker.Application.Behaviors;

/// <summary>
/// Runs FluentValidation validators before the handler. On failure it short-circuits
/// with a failed Result carrying a <see cref="ValidationError"/> — no exceptions
/// ("errors as values", consistent with the rest of the app).
///
/// The <c>where TResponse : ResultBase</c> constraint makes MediatR apply this behavior
/// only to commands returning Result / Result&lt;T&gt;; queries (raw return types) are skipped.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : ResultBase
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            var results = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = results
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Count != 0)
            {
                return CreateFailedResult(ValidationError.From(failures));
            }
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateFailedResult(IError error)
    {
        // TResponse is either Result or Result<T>.
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(error);
        }

        // Result<T> — build Result.Fail<T>(error) for the closed value type.
        var valueType = typeof(TResponse).GetGenericArguments()[0];

        var failGeneric = typeof(Result)
            .GetMethods()
            .First(m => m.Name == nameof(Result.Fail)
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == typeof(IError))
            .MakeGenericMethod(valueType);

        return (TResponse)failGeneric.Invoke(null, new object[] { error })!;
    }
}

using FluentValidation.Results;

namespace KomTracker.Application.Errors;

/// <summary>
/// Input validation failure (FluentValidation). Maps to HTTP 422.
/// Carries per-field messages for a ValidationProblemDetails response.
/// </summary>
public class ValidationError : AppError
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationError(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }

    /// <summary>Build from FluentValidation failures, keying by the (camelCased) field name.</summary>
    public static ValidationError From(IEnumerable<ValidationFailure> failures)
    {
        var errorsByField = failures
            .GroupBy(f => ToFieldKey(f.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());

        return new ValidationError(errorsByField);
    }

    // camelCase every segment of the property path so keys match the posted JSON (e.g. "saleDate").
    private static string ToFieldKey(string propertyName)
        => string.IsNullOrEmpty(propertyName)
            ? propertyName
            : string.Join('.', propertyName.Split('.').Select(CamelCaseSegment));

    private static string CamelCaseSegment(string segment)
        => string.IsNullOrEmpty(segment) || char.IsLower(segment[0])
            ? segment
            : char.ToLowerInvariant(segment[0]) + segment[1..];
}

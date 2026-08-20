namespace KomTracker.Application.Commands.Component;

internal static class ComponentDateHelper
{
    /// <summary>
    /// Coerce an incoming date to UTC kind for timestamptz persistence (Npgsql modern mode).
    /// Dates carry no meaningful time component here, so we relabel rather than convert.
    /// </summary>
    public static DateTime? EnsureUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}

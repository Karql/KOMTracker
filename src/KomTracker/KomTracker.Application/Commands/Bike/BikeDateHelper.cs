namespace KomTracker.Application.Commands.Bike;

internal static class BikeDateHelper
{
    /// <summary>
    /// Coerce an incoming date to UTC kind for timestamptz persistence (Npgsql modern mode).
    /// Dates carry no meaningful time component here, so we relabel rather than convert.
    /// </summary>
    public static DateTime? EnsureUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}

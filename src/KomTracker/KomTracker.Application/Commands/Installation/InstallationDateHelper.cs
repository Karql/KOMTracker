namespace KomTracker.Application.Commands.Installation;

internal static class InstallationDateHelper
{
    /// <summary>Relabel an incoming date to UTC kind for timestamptz persistence (Npgsql modern mode).</summary>
    public static DateTime? EnsureUtc(DateTime? value)
        => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}

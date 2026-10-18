namespace KomTracker.Application.Commands;

/// <summary>Shared texts of the delete guards, so the WEB can recognise a block that no "delete anyway" can lift.</summary>
public static class DeleteBlockMessages
{
    /// <summary>Marker the WEB looks for in the problem detail (see <see cref="CoveredByService"/>).</summary>
    public const string ServiceMarker = "covered by a service";

    public static string CoveredByService(string what)
        => $"{what} is {ServiceMarker} — remove it from those services (or delete them) first.";
}

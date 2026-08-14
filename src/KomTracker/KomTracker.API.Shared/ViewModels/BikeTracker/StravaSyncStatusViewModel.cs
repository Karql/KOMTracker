namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>Per-athlete Strava sync state for the "Strava bikes" page (status line + empty-state).</summary>
public class StravaSyncStatusViewModel
{
    public bool BikesEnabled { get; set; }
    public bool ActivitiesEnabled { get; set; }
    public bool HasActivityReadAll { get; set; }
    public int StravaBikeCount { get; set; }

    /// <summary>Strava scopes currently granted on the athlete's token (for display).</summary>
    public string[] Scopes { get; set; } = System.Array.Empty<string>();
}

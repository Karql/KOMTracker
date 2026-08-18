namespace KomTracker.Application.Models.Strava;

/// <summary>Per-athlete Strava sync state driving the "Strava bikes" page (status + empty-state).</summary>
public class StravaSyncStatusModel
{
    public bool BikesEnabled { get; set; }
    public bool ActivitiesEnabled { get; set; }
    public bool HasActivityReadAll { get; set; }
    public int StravaBikeCount { get; set; }
    public int ActivityCount { get; set; }

    /// <summary>The Strava scopes currently granted on the athlete's token (for display).</summary>
    public string[] Scopes { get; set; } = System.Array.Empty<string>();
}

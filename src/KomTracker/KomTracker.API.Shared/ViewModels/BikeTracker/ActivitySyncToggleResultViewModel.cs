namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>Result of toggling activity auto-sync — tells the UI a first-time background backfill was kicked.</summary>
public class ActivitySyncToggleResultViewModel
{
    public bool BackfillStarted { get; set; }
}

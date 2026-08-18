using System;

namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>One activity-sync run, for the sync-history dialog.</summary>
public class ActivitySyncHistoryViewModel
{
    public DateTime RunAt { get; set; }
    public TimeSpan Duration { get; set; }
    /// <summary>Window start; null = full pull.</summary>
    public DateTime? SyncFrom { get; set; }
    public string Status { get; set; } = default!;
    public int UpsertedCount { get; set; }
    public int DeletedCount { get; set; }
    public int? ActivitiesCount { get; set; }
}

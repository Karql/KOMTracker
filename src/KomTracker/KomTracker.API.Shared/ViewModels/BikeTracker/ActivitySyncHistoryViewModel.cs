using System;
using KomTracker.Domain.Entities.Strava;

namespace KomTracker.API.Shared.ViewModels.BikeTracker;

/// <summary>One activity-sync run, for the sync-history dialog.</summary>
public class ActivitySyncHistoryViewModel
{
    public DateTime RunAt { get; set; }
    public TimeSpan Duration { get; set; }
    /// <summary>What triggered the run — a Job or a Webhook.</summary>
    public ActivitySyncType Type { get; set; }
    /// <summary>The single activity a Webhook run synced (null for Job runs).</summary>
    public long? ActivityId { get; set; }
    /// <summary>Window start; null = full pull (Job runs only).</summary>
    public DateTime? SyncFrom { get; set; }
    public string Status { get; set; } = default!;
    public int UpsertedCount { get; set; }
    public int DeletedCount { get; set; }
    public int? ActivitiesCount { get; set; }
}

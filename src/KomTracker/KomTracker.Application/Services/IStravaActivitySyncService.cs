using FluentResults;

namespace KomTracker.Application.Services;

/// <summary>
/// Single-activity Strava sync operations (the unit of work behind <see cref="Commands.Strava.SyncActivityCommand"/>
/// and the webhook drain). Extracted so a batch/handler that needs the per-activity <see cref="Result"/> can call it
/// directly instead of one command sending another.
/// </summary>
public interface IStravaActivitySyncService
{
    /// <summary>Fetch one activity from Strava and upsert it (create/update), then announce it for mileage recompute.</summary>
    Task<Result> SyncAthleteActivityAsync(int athleteId, long activityId, CancellationToken cancellationToken = default);

    /// <summary>Remove a locally-stored activity (webhook delete), then announce it so mileage drops the ride.</summary>
    Task<Result> DeleteAthleteActivityAsync(int athleteId, long activityId, CancellationToken cancellationToken = default);
}

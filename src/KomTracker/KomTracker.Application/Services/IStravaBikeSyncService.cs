using FluentResults;

namespace KomTracker.Application.Services;

/// <summary>
/// Syncs one athlete's Strava gear (bikes, incl. retired) into <c>strava.bike</c> (1:1 mirror). The unit of work behind
/// both <see cref="Commands.Strava.SyncStravaBikesCommand"/> (single-athlete entry point for controllers) and the
/// <see cref="Commands.Strava.SyncBikesCommand"/> batch loop — extracted so the batch can drive it directly and inspect
/// each athlete's <see cref="Result"/> (rate-limit → stop the run) instead of one command sending another.
/// </summary>
public interface IStravaBikeSyncService
{
    Task<Result> SyncAthleteBikesAsync(int athleteId, CancellationToken cancellationToken);
}

using FluentResults;
using KomTracker.Application.Services;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Sync one athlete's Strava gear (bikes, incl. retired) into strava.bike (1:1 mirror). Gear only —
/// does NOT touch any auto-sync flag (bike auto-sync is toggled on Account) nor activity sync.
/// Surfaces rate-limit / auth failures to the caller. Single-athlete entry point (controllers); the actual sync
/// lives in <see cref="IStravaBikeSyncService"/> so <see cref="SyncBikesCommand"/> can drive it directly.
/// </summary>
public class SyncStravaBikesCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
}

public class SyncStravaBikesCommandHandler : IRequestHandler<SyncStravaBikesCommand, Result>
{
    private readonly IStravaBikeSyncService _bikeSyncService;

    public SyncStravaBikesCommandHandler(IStravaBikeSyncService bikeSyncService)
    {
        _bikeSyncService = bikeSyncService ?? throw new ArgumentNullException(nameof(bikeSyncService));
    }

    public Task<Result> Handle(SyncStravaBikesCommand request, CancellationToken cancellationToken)
        => _bikeSyncService.SyncAthleteBikesAsync(request.AthleteId, cancellationToken);
}

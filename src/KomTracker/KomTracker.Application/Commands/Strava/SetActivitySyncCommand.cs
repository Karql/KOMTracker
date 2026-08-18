using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Enable/disable per-athlete automatic activity sync (gates the activity jobs). Returns
/// <see cref="SetActivitySyncResult.BackfillNeeded"/> = true only on the FIRST-ever enable
/// (no history yet) so the caller can kick a one-time background backfill; toggling off/on
/// later never re-backfills (rate-limit protection). Does not touch bike sync.
/// </summary>
public class SetActivitySyncCommand : IRequest<Result<SetActivitySyncResult>>
{
    public int AthleteId { get; set; }
    public bool Enabled { get; set; }
}

public class SetActivitySyncResult
{
    public bool BackfillNeeded { get; set; }
}

public class SetActivitySyncCommandHandler : IRequestHandler<SetActivitySyncCommand, Result<SetActivitySyncResult>>
{
    private readonly IKOMUnitOfWork _komUoW;

    public SetActivitySyncCommandHandler(IKOMUnitOfWork komUoW)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<Result<SetActivitySyncResult>> Handle(SetActivitySyncCommand request, CancellationToken cancellationToken)
    {
        var athleteSyncRepo = _komUoW.GetRepository<IAthleteSyncRepository>();
        var historyRepo = _komUoW.GetRepository<IActivitySyncHistoryRepository>();

        var backfillNeeded = request.Enabled && !await historyRepo.AnyForAthleteAsync(request.AthleteId);

        await athleteSyncRepo.SetActivitiesEnabledAsync(request.AthleteId, request.Enabled);

        return Result.Ok(new SetActivitySyncResult { BackfillNeeded = backfillNeeded });
    }
}

using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Strava;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Enable/disable per-athlete Strava activity sync. On a fresh enable, kicks a best-effort full backfill
/// for that athlete (the flag alone would only take effect on the next scheduled job).
/// </summary>
public class SetAthleteSyncCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
    public bool Enabled { get; set; }
}

public class SetAthleteSyncCommandHandler : IRequestHandler<SetAthleteSyncCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IMediator _mediator;
    private readonly ILogger<SetAthleteSyncCommandHandler> _logger;

    public SetAthleteSyncCommandHandler(IKOMUnitOfWork komUoW, IMediator mediator, ILogger<SetAthleteSyncCommandHandler> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(SetAthleteSyncCommand request, CancellationToken cancellationToken)
    {
        var repo = _komUoW.GetRepository<IAthleteSyncRepository>();

        var current = await repo.GetAsync(request.AthleteId);
        var wasEnabled = current?.ActivitiesEnabled ?? false;

        await repo.UpsertAsync(new AthleteSyncEntity
        {
            AthleteId = request.AthleteId,
            ActivitiesEnabled = request.Enabled
        });

        // Fresh enable → initial full backfill for this athlete only. Best-effort: a failure here
        // (e.g. rate limit) must not undo the opt-in — the scheduled job will catch up.
        if (request.Enabled && !wasEnabled)
        {
            var backfill = await _mediator.Send(new SyncActivitiesCommand { After = null, AthleteId = request.AthleteId }, cancellationToken);
            if (backfill.IsFailed)
            {
                _logger.LogWarning("Initial activity backfill for athlete {athleteId} did not complete: {errors}",
                    request.AthleteId, string.Join("; ", backfill.Errors.Select(e => e.Message)));
            }
        }

        return Result.Ok();
    }
}

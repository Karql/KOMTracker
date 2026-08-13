using FluentResults;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// The single user-facing "Sync from Strava" action: sync the athlete's gear (strava.bike) and
/// enable activity sync (which backfills on a fresh enable). Aggregates the two composable
/// sub-commands so the UI is one click while the internals stay independently usable.
/// </summary>
public class ActivateStravaSyncCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
    public string UserId { get; set; } = default!;
}

public class ActivateStravaSyncCommandHandler : IRequestHandler<ActivateStravaSyncCommand, Result>
{
    private readonly IMediator _mediator;

    public ActivateStravaSyncCommandHandler(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<Result> Handle(ActivateStravaSyncCommand request, CancellationToken cancellationToken)
    {
        // Gear sync first — its failure (rate limit / auth) is the one the user needs surfaced.
        var bikesRes = await _mediator.Send(new SyncStravaBikesCommand { AthleteId = request.AthleteId }, cancellationToken);
        if (bikesRes.IsFailed)
        {
            return bikesRes;
        }

        // Enable activity sync (fresh enable → best-effort backfill inside the handler).
        return await _mediator.Send(new SetAthleteSyncCommand { AthleteId = request.AthleteId, Enabled = true }, cancellationToken);
    }
}

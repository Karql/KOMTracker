using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using IStravaActivityService = KomTracker.Application.Interfaces.Services.Strava.IActivityService;
using StravaActivitiesError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteActivitiesError;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Sync a single Strava activity (GET /activities/{id}) and upsert it — a targeted refresh of one row.
/// Atomic primitive keyed only by ids, with its own token acquisition and no HTTP/session coupling, so it is
/// reusable verbatim by a future Strava webhook handler (create/update events) — see the phase-1e-iter-2 spec.
/// </summary>
public class SyncActivityCommand : IRequest<Result>
{
    public int AthleteId { get; set; }

    public long ActivityId { get; set; }
}

public class SyncActivityCommandHandler : IRequestHandler<SyncActivityCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaActivityService _activityService;
    private readonly ILogger<SyncActivityCommandHandler> _logger;

    public SyncActivityCommandHandler(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaActivityService activityService, ILogger<SyncActivityCommandHandler> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(SyncActivityCommand request, CancellationToken cancellationToken)
    {
        var tokenRes = await _athleteService.GetValidTokenAsync(request.AthleteId);
        if (tokenRes.IsFailed)
        {
            return Result.Fail($"{nameof(SyncActivityCommand)}: no valid token for athlete {request.AthleteId}.");
        }

        var activityRes = await _activityService.GetAthleteActivityAsync(request.AthleteId, tokenRes.Value.AccessToken, request.ActivityId);
        if (activityRes.IsFailed)
        {
            var msg = activityRes.Errors.OfType<StravaActivitiesError>().FirstOrDefault()?.Message;
            if (msg == StravaActivitiesError.NotFound)
            {
                return Result.Fail(new NotFoundError($"Activity {request.ActivityId} not found for athlete {request.AthleteId}."));
            }

            _logger.LogError("{command} failed for athlete {athleteId}, activity {activityId}: {error}",
                nameof(SyncActivityCommand), request.AthleteId, request.ActivityId, msg);
            return Result.Fail($"{nameof(SyncActivityCommand)} failed ({msg ?? "unknown"}).");
        }

        var activityRepo = _komUoW.GetRepository<IActivityRepository>();
        await activityRepo.UpsertActivityAsync(activityRes.Value);

        return Result.Ok();
    }
}

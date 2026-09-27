using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Notifications.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using IStravaActivityService = KomTracker.Application.Interfaces.Services.Strava.IActivityService;
using StravaActivitiesError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteActivitiesError;

namespace KomTracker.Application.Services;

public class StravaActivitySyncService : IStravaActivitySyncService
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaActivityService _activityService;
    private readonly IMediator _mediator;
    private readonly ILogger<StravaActivitySyncService> _logger;

    public StravaActivitySyncService(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaActivityService activityService, IMediator mediator, ILogger<StravaActivitySyncService> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> SyncAthleteActivityAsync(int athleteId, long activityId, CancellationToken cancellationToken = default)
    {
        var tokenRes = await _athleteService.GetValidTokenAsync(athleteId);
        if (tokenRes.IsFailed)
        {
            return Result.Fail($"No valid Strava token for athlete {athleteId}.");
        }

        var activityRes = await _activityService.GetAthleteActivityAsync(athleteId, tokenRes.Value.AccessToken, activityId);
        if (activityRes.IsFailed)
        {
            var msg = activityRes.Errors.OfType<StravaActivitiesError>().FirstOrDefault()?.Message;
            if (msg == StravaActivitiesError.NotFound)
            {
                return Result.Fail(new NotFoundError($"Activity {activityId} not found for athlete {athleteId}."));
            }

            _logger.LogError("SyncAthleteActivity failed for athlete {athleteId}, activity {activityId}: {error}",
                athleteId, activityId, msg);
            return Result.Fail($"SyncAthleteActivity failed ({msg ?? "unknown"}).");
        }

        var activity = activityRes.Value;
        await _komUoW.GetRepository<IActivityRepository>().UpsertActivityAsync(activity);

        // Announce the sync; the projection updater recomputes the components on this ride's bike (gear → bike → components).
        await _mediator.Publish(new ActivitySyncedNotification
        {
            AthleteId = athleteId,
            ActivityId = activityId,
            GearId = activity.GearId
        }, cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> DeleteAthleteActivityAsync(int athleteId, long activityId, CancellationToken cancellationToken = default)
    {
        var activityRepo = _komUoW.GetRepository<IActivityRepository>();

        // Capture the gear BEFORE deleting — the notification needs it to resolve which components to recompute
        // (Strava won't return the activity anymore, and the webhook payload has no gear id).
        var existing = await activityRepo.GetAsync(athleteId, activityId);
        var gearId = existing?.GearId;

        await activityRepo.DeleteAsync(athleteId, activityId);

        // Delete-before-publish: the recompute reads from the now-reduced activity set, so the ride's mileage drops.
        await _mediator.Publish(new ActivitySyncedNotification
        {
            AthleteId = athleteId,
            ActivityId = activityId,
            GearId = gearId
        }, cancellationToken);

        return Result.Ok();
    }
}

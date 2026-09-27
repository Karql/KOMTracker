using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Drains the Strava webhook inbox (`strava.webhook_event`, D-12): for each unprocessed activity event, create/update
/// → re-sync the activity by id, delete → remove it locally; both recompute mileage via the sync service. Only acts on
/// object_type=activity for athletes with activity sync enabled; everything else is marked processed as a no-op.
/// Terminal outcomes (synced / NotFound / skipped) mark the event processed; transient failures are left for the next run.
/// </summary>
public class ProcessStravaWebhookEventsCommand : IRequest<Result>
{
}

public class ProcessStravaWebhookEventsCommandHandler : IRequestHandler<ProcessStravaWebhookEventsCommand, Result>
{
    private const string ObjectTypeActivity = "activity";
    private const string AspectCreate = "create";
    private const string AspectUpdate = "update";
    private const string AspectDelete = "delete";

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IStravaActivitySyncService _activitySyncService;
    private readonly ILogger<ProcessStravaWebhookEventsCommandHandler> _logger;

    public ProcessStravaWebhookEventsCommandHandler(IKOMUnitOfWork komUoW, IStravaActivitySyncService activitySyncService, ILogger<ProcessStravaWebhookEventsCommandHandler> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _activitySyncService = activitySyncService ?? throw new ArgumentNullException(nameof(activitySyncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(ProcessStravaWebhookEventsCommand request, CancellationToken cancellationToken)
    {
        var webhookRepo = _komUoW.GetRepository<IWebhookEventRepository>();
        var events = await webhookRepo.GetUnprocessedAsync();

        foreach (var webhookEvent in events)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                await ProcessOneAsync(webhookEvent, cancellationToken);
            }
            catch (Exception ex)
            {
                // Leave it unprocessed — the next run (hourly or next trigger) retries.
                _logger.LogError(ex, "Failed processing webhook event {id} (object {objectType}/{objectId}, {aspect}) — will retry.",
                    webhookEvent.Id, webhookEvent.ObjectType, webhookEvent.ObjectId, webhookEvent.AspectType);
            }
        }

        return Result.Ok();
    }

    private async Task ProcessOneAsync(WebhookEventEntity webhookEvent, CancellationToken cancellationToken)
    {
        var webhookRepo = _komUoW.GetRepository<IWebhookEventRepository>();

        // Only activity events have handlers today; athlete/unknown → no-op, mark processed so they don't linger.
        if (!string.Equals(webhookEvent.ObjectType, ObjectTypeActivity, StringComparison.OrdinalIgnoreCase))
        {
            await webhookRepo.MarkProcessedAsync(webhookEvent.Id);
            return;
        }

        var athleteId = (int)webhookEvent.OwnerId;

        // Skip athletes without activity sync (opted out / not tracked). No history row for a skip.
        var sync = await _komUoW.GetRepository<IAthleteSyncRepository>().GetAsync(athleteId);
        if (sync?.ActivitiesEnabled != true)
        {
            await webhookRepo.MarkProcessedAsync(webhookEvent.Id);
            return;
        }

        var runStartedAt = DateTime.UtcNow;
        string status;
        var upserted = 0;
        var deleted = 0;

        var aspect = webhookEvent.AspectType?.ToLowerInvariant();
        switch (aspect)
        {
            case AspectCreate:
            case AspectUpdate:
            {
                var res = await _activitySyncService.SyncAthleteActivityAsync(athleteId, webhookEvent.ObjectId, cancellationToken);
                if (res.IsSuccess)
                {
                    status = "Ok";
                    upserted = 1;
                }
                else if (res.HasError<NotFoundError>())
                {
                    // Permanent — the activity is gone/private on Strava. Record + stop retrying.
                    status = "NotFound";
                }
                else
                {
                    // Transient (no token, rate-limit, other) — leave unprocessed to retry.
                    _logger.LogWarning("Webhook event {id}: activity {activityId} sync deferred — {errors}",
                        webhookEvent.Id, webhookEvent.ObjectId, string.Join("; ", res.Errors.Select(e => e.Message)));
                    return;
                }

                break;
            }

            case AspectDelete:
            {
                await _activitySyncService.DeleteAthleteActivityAsync(athleteId, webhookEvent.ObjectId, cancellationToken);
                status = "Deleted";
                deleted = 1;
                break;
            }

            default:
            {
                // Unknown aspect — nothing to do.
                await webhookRepo.MarkProcessedAsync(webhookEvent.Id);
                return;
            }
        }

        _komUoW.GetRepository<IActivitySyncHistoryRepository>().Add(new ActivitySyncHistoryEntity
        {
            AthleteId = athleteId,
            Type = ActivitySyncType.Webhook,
            ActivityId = webhookEvent.ObjectId,
            RunAt = runStartedAt,
            Duration = DateTime.UtcNow - runStartedAt,
            SyncFrom = null,
            Status = status,
            UpsertedCount = upserted,
            DeletedCount = deleted,
            ActivitiesCount = null
        });
        await _komUoW.SaveChangesAsync();

        await webhookRepo.MarkProcessedAsync(webhookEvent.Id);
    }
}

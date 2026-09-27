using FluentResults;
using KomTracker.Application.Errors;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using MediatR;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Sync a single Strava activity (GET /activities/{id}) and upsert it — a targeted manual refresh of one row (Refresh
/// button). Delegates the sync to <see cref="IStravaActivitySyncService"/> (shared with the webhook drain) and records
/// a <see cref="ActivitySyncType.Manual"/> row in the sync history.
/// </summary>
public class SyncActivityCommand : IRequest<Result>
{
    public int AthleteId { get; set; }

    public long ActivityId { get; set; }
}

public class SyncActivityCommandHandler : IRequestHandler<SyncActivityCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IStravaActivitySyncService _activitySyncService;

    public SyncActivityCommandHandler(IKOMUnitOfWork komUoW, IStravaActivitySyncService activitySyncService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _activitySyncService = activitySyncService ?? throw new ArgumentNullException(nameof(activitySyncService));
    }

    public async Task<Result> Handle(SyncActivityCommand request, CancellationToken cancellationToken)
    {
        var runStartedAt = DateTime.UtcNow;
        var res = await _activitySyncService.SyncAthleteActivityAsync(request.AthleteId, request.ActivityId, cancellationToken);

        // Record a Manual history row on terminal outcomes (Ok / NotFound); a transient failure is surfaced to the
        // caller (snackbar) without a history entry.
        if (res.IsSuccess || res.HasError<NotFoundError>())
        {
            _komUoW.GetRepository<IActivitySyncHistoryRepository>().Add(new ActivitySyncHistoryEntity
            {
                AthleteId = request.AthleteId,
                Type = ActivitySyncType.Manual,
                ActivityId = request.ActivityId,
                RunAt = runStartedAt,
                Duration = DateTime.UtcNow - runStartedAt,
                SyncFrom = null,
                Status = res.IsSuccess ? "Ok" : "NotFound",
                UpsertedCount = res.IsSuccess ? 1 : 0,
                DeletedCount = 0,
                ActivitiesCount = null
            });
            await _komUoW.SaveChangesAsync();
        }

        return res;
    }
}

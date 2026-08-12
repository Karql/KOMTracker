using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using IStravaActivityService = KomTracker.Application.Interfaces.Services.Strava.IActivityService;
using StravaActivitiesError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteActivitiesError;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Sync Strava activities for every opted-in athlete (`strava.athlete_sync.Enabled`).
/// <see cref="After"/> null ⇒ full pull (all pages); set ⇒ windowed (`after=<epoch>`). The window
/// (and thus the delete-detection scope) is decided by the caller/job.
/// </summary>
public class SyncActivitiesCommand : IRequest<Result>
{
    public DateTime? After { get; set; }
}

public class SyncActivitiesCommandHandler : IRequestHandler<SyncActivitiesCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaActivityService _activityService;
    private readonly ILogger<SyncActivitiesCommandHandler> _logger;

    public SyncActivitiesCommandHandler(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaActivityService activityService, ILogger<SyncActivitiesCommandHandler> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(SyncActivitiesCommand request, CancellationToken cancellationToken)
    {
        var after = request.After.HasValue ? DateTime.SpecifyKind(request.After.Value, DateTimeKind.Utc) : (DateTime?)null;
        var afterEpoch = after.HasValue ? new DateTimeOffset(after.Value).ToUnixTimeSeconds() : (long?)null;

        var athleteSyncRepo = _komUoW.GetRepository<IAthleteSyncRepository>();
        var activityRepo = _komUoW.GetRepository<IActivityRepository>();
        var historyRepo = _komUoW.GetRepository<IActivitySyncHistoryRepository>();

        var athleteIds = await athleteSyncRepo.GetActivitiesEnabledAthleteIdsAsync();

        foreach (var athleteId in athleteIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Ok();
            }

            _komUoW.ClearChangeTracker();

            var runStartedAt = DateTime.UtcNow;

            try
            {
                var tokenRes = await _athleteService.GetValidTokenAsync(athleteId);
                if (tokenRes.IsFailed)
                {
                    await RecordHistoryAsync(historyRepo, athleteId, runStartedAt, after, "NoValidToken", 0, 0, null);
                    continue;
                }

                var token = tokenRes.Value;

                var activitiesRes = await _activityService.GetAthleteActivitiesAsync(athleteId, token.AccessToken, afterEpoch);
                if (activitiesRes.IsFailed)
                {
                    var msg = activitiesRes.Errors.OfType<StravaActivitiesError>().FirstOrDefault()?.Message;
                    if (msg == StravaActivitiesError.TooManyRequests)
                    {
                        // Shared rate bucket — record and stop the whole run to back off.
                        await RecordHistoryAsync(historyRepo, athleteId, runStartedAt, after, "RateLimited", 0, 0, null);
                        return Result.Fail($"{nameof(SyncActivitiesCommand)} interrupted (rate limit)!");
                    }

                    await RecordHistoryAsync(historyRepo, athleteId, runStartedAt, after, "Error", 0, 0, null);
                    continue;
                }

                var activities = activitiesRes.Value.ToList();
                var deleted = await activityRepo.UpsertAthleteActivitiesAsync(athleteId, activities, after);
                var total = await activityRepo.CountAthleteActivitiesAsync(athleteId);

                await RecordHistoryAsync(historyRepo, athleteId, runStartedAt, after, "Ok", activities.Count, deleted, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{command} failed for athlete {athleteId} - skipping", nameof(SyncActivitiesCommand), athleteId);
            }
        }

        return Result.Ok();
    }

    private async Task RecordHistoryAsync(IActivitySyncHistoryRepository historyRepo, int athleteId, DateTime runStartedAt, DateTime? syncFrom, string status, int upserted, int deleted, int? activitiesCount)
    {
        historyRepo.Add(new ActivitySyncHistoryEntity
        {
            AthleteId = athleteId,
            RunAt = runStartedAt,
            Duration = DateTime.UtcNow - runStartedAt,
            SyncFrom = syncFrom,
            Status = status,
            UpsertedCount = upserted,
            DeletedCount = deleted,
            ActivitiesCount = activitiesCount
        });

        await _komUoW.SaveChangesAsync();
    }
}

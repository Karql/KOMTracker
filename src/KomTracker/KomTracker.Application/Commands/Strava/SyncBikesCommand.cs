using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using StravaGearError = KomTracker.Application.Interfaces.Services.Strava.GetAthleteBikesError;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Sync Strava bikes for every bike-auto-sync-enabled athlete (or a single one). Per-athlete isolation
/// (a bad athlete is skipped); a shared-bucket rate-limit stops the whole run. Used by the bike job.
/// </summary>
public class SyncBikesCommand : IRequest<Result>
{
    /// <summary>When set, sync only this athlete; null = all athletes with bike auto-sync enabled.</summary>
    public int? AthleteId { get; set; }
}

public class SyncBikesCommandHandler : IRequestHandler<SyncBikesCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IStravaBikeSyncService _bikeSyncService;
    private readonly ILogger<SyncBikesCommandHandler> _logger;

    public SyncBikesCommandHandler(IKOMUnitOfWork komUoW, IStravaBikeSyncService bikeSyncService, ILogger<SyncBikesCommandHandler> logger)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _bikeSyncService = bikeSyncService ?? throw new ArgumentNullException(nameof(bikeSyncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(SyncBikesCommand request, CancellationToken cancellationToken)
    {
        var athleteSyncRepo = _komUoW.GetRepository<IAthleteSyncRepository>();

        var athleteIds = request.AthleteId.HasValue
            ? new[] { request.AthleteId.Value }.AsEnumerable()
            : await athleteSyncRepo.GetBikesEnabledAthleteIdsAsync();

        foreach (var athleteId in athleteIds)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Result.Ok();
            }

            _komUoW.ClearChangeTracker();

            try
            {
                var res = await _bikeSyncService.SyncAthleteBikesAsync(athleteId, cancellationToken);
                if (res.IsFailed)
                {
                    var rateLimited = res.Errors
                        .OfType<StravaGearError>()
                        .Any(e => e.Message == StravaGearError.TooManyRequests);

                    if (rateLimited)
                    {
                        return Result.Fail($"{nameof(SyncBikesCommand)} interrupted (rate limit)!");
                    }

                    _logger.LogWarning("{command} skipped athlete {athleteId}: {errors}",
                        nameof(SyncBikesCommand), athleteId, string.Join("; ", res.Errors.Select(e => e.Message)));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{command} failed for athlete {athleteId} - skipping", nameof(SyncBikesCommand), athleteId);
            }
        }

        return Result.Ok();
    }
}

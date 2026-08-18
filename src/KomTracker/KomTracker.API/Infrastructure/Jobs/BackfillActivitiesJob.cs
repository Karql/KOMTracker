using KomTracker.Application.Commands.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

/// <summary>
/// One-shot full activity backfill for a single athlete (read from the job data map). Fired in the
/// background right after an athlete first enables activity sync, so they don't wait for the nightly job.
/// Scheduled with a per-athlete key + DisallowConcurrentExecution so toggling on/off can't start a second run.
/// </summary>
[DisallowConcurrentExecution]
public class BackfillActivitiesJob : IJob
{
    public const string AthleteIdKey = "athleteId";

    public static JobKey KeyFor(int athleteId) => new($"backfill-activities-{athleteId}");

    private readonly ILogger<BackfillActivitiesJob> _logger;
    private readonly IMediator _mediator;

    public BackfillActivitiesJob(ILogger<BackfillActivitiesJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var athleteId = context.MergedJobDataMap.GetInt(AthleteIdKey);
        _logger.LogInformation("Start job: {job} for athlete {athleteId}", nameof(BackfillActivitiesJob), athleteId);

        await _mediator.Send(new SyncActivitiesCommand { After = null, AthleteId = athleteId }, context.CancellationToken);
    }
}

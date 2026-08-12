using KomTracker.Application.Commands.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

/// <summary>Recent-window Strava activity sync (last <see cref="RecentWindowDays"/> days) — daily.</summary>
[DisallowConcurrentExecution]
public class SyncActivitiesRecentJob : IJob
{
    private const int RecentWindowDays = 7;

    private readonly ILogger<SyncActivitiesRecentJob> _logger;
    private readonly IMediator _mediator;

    public SyncActivitiesRecentJob(ILogger<SyncActivitiesRecentJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(SyncActivitiesRecentJob));
        await _mediator.Send(new SyncActivitiesCommand { After = DateTime.UtcNow.AddDays(-RecentWindowDays) }, context.CancellationToken);
    }
}

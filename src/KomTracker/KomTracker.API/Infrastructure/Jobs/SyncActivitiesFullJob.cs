using KomTracker.Application.Commands.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

/// <summary>Full Strava activity sync (all pages, delete-detection) — weekly.</summary>
[DisallowConcurrentExecution]
public class SyncActivitiesFullJob : IJob
{
    private readonly ILogger<SyncActivitiesFullJob> _logger;
    private readonly IMediator _mediator;

    public SyncActivitiesFullJob(ILogger<SyncActivitiesFullJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(SyncActivitiesFullJob));
        await _mediator.Send(new SyncActivitiesCommand { After = null }, context.CancellationToken);
    }
}

using KomTracker.Application.Commands.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

/// <summary>Automatic Strava bike (gear) sync for all bike-auto-sync-enabled athletes — daily.</summary>
[DisallowConcurrentExecution]
public class SyncBikesJob : IJob
{
    private readonly ILogger<SyncBikesJob> _logger;
    private readonly IMediator _mediator;

    public SyncBikesJob(ILogger<SyncBikesJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(SyncBikesJob));
        await _mediator.Send(new SyncBikesCommand(), context.CancellationToken);
    }
}

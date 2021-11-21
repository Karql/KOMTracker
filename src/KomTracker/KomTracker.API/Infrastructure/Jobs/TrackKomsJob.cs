using KomTracker.Application.Commands.Tracking;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class TrackKomsJob : IJob
{
    private readonly ILogger<TrackKomsJob> _logger;
    private readonly IMediator _mediator;

    public TrackKomsJob(ILogger<TrackKomsJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(TrackKomsJob));
        await _mediator.Send(new TrackKomsCommand(), context.CancellationToken);
    }
}

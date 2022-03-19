using KomTracker.Application.Commands.Tracking;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RefreshSegmentsJob : IJob
{
    private readonly ILogger<RefreshSegmentsJob> _logger;
    private readonly IMediator _mediator;

    public RefreshSegmentsJob(ILogger<RefreshSegmentsJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(RefreshSegmentsJob));
        await _mediator.Send(new RefreshSegmentsCommand(), context.CancellationToken);
    }
}

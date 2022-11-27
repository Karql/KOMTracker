using KomTracker.Application.Commands.Stats;
using MediatR;
using Quartz;

namespace KomTracker.API.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RefreshStatsJob : IJob
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public RefreshStatsJob(ILogger<RefreshClubsJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(RefreshStatsJob));
        await _mediator.Send(new RefreshStatsCommand(), context.CancellationToken);
    }
}
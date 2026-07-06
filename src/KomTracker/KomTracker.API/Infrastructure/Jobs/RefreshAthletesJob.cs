using KomTracker.Application.Commands.Account;
using MediatR;
using Quartz;

namespace KomTracker.API.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RefreshAthletesJob : IJob
{
    private readonly ILogger _logger;
    private readonly IMediator _mediator;

    public RefreshAthletesJob(ILogger<RefreshAthletesJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(RefreshAthletesJob));
        await _mediator.Send(new RefreshAthletesCommand(), context.CancellationToken);
    }
}

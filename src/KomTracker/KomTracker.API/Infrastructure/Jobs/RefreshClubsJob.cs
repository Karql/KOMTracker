using KomTracker.Application.Commands.Club;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class RefreshClubsJob : IJob
{
    private readonly ILogger<RefreshClubsJob> _logger;
    private readonly IMediator _mediator;

    public RefreshClubsJob(ILogger<RefreshClubsJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(RefreshClubsJob));
        await _mediator.Send(new RefreshClubsCommand(), context.CancellationToken);
    }
}

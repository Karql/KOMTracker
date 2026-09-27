using KomTracker.Application.Commands.Strava;
using MediatR;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

/// <summary>
/// Drains the Strava webhook inbox (D-12). Runs hourly (backstop) and is also triggered immediately when a webhook
/// arrives. <see cref="DisallowConcurrentExecutionAttribute"/> means a trigger fired mid-run is blocked and runs right
/// after — never dropped — so events aren't delayed to the next hourly tick.
/// </summary>
[DisallowConcurrentExecution]
public class ProcessStravaWebhookEventsJob : IJob
{
    /// <summary>Stable key so the webhook controller can trigger this durable job on receipt.</summary>
    public static readonly JobKey Key = new(nameof(ProcessStravaWebhookEventsJob));

    private readonly ILogger<ProcessStravaWebhookEventsJob> _logger;
    private readonly IMediator _mediator;

    public ProcessStravaWebhookEventsJob(ILogger<ProcessStravaWebhookEventsJob> logger, IMediator mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(ProcessStravaWebhookEventsJob));
        await _mediator.Send(new ProcessStravaWebhookEventsCommand(), context.CancellationToken);
    }
}

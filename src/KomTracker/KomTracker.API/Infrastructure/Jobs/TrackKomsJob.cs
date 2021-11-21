using KomTracker.Application.Services;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace KomTracker.API.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class TrackKomsJob : IJob
{
    private readonly ILogger<TrackKomsJob> _logger;
    private readonly IKomService _komService;

    public TrackKomsJob(ILogger<TrackKomsJob> logger, IKomService komService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _komService = komService ?? throw new ArgumentNullException(nameof(komService));
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogDebug("Start job: {job}", nameof(TrackKomsJob));
        await _komService.TrackKomsAsync(context.CancellationToken);
    }
}

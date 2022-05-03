using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using MediatR;
using Microsoft.Extensions.Logging;
using IStravaSegmentService = KomTracker.Application.Interfaces.Services.Strava.ISegmentService;

namespace KomTracker.Application.Commands.Tracking;

public class RefreshSegmentsCommand : IRequest<Result>
{
    public int SegmentsToRefresh { get; set; } = 100;

    public TimeSpan? MinimumTimeFromLastRefresh { get; set; }
}

public class RefreshSegmentsCommandHandler : IRequestHandler<RefreshSegmentsCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<RefreshSegmentsCommandHandler> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly ISegmentService _segmentService;
    private readonly IAthleteService _athleteService;
    private readonly IStravaSegmentService _stravaSegmentService;

    public RefreshSegmentsCommandHandler(IKOMUnitOfWork komUoW, ILogger<RefreshSegmentsCommandHandler> logger, ApplicationConfiguration applicationConfiguration, ISegmentService segmentService, IAthleteService athleteService, IStravaSegmentService stravaSegmentService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaSegmentService = stravaSegmentService ?? throw new ArgumentNullException(nameof(stravaSegmentService));
    }

    public async Task<Result> Handle(RefreshSegmentsCommand request, CancellationToken cancellationToken)
    {
        var masterStravaAthleteId = _applicationConfiguration.MasterStravaAthleteId;

        var token = await GetTokenAsync(masterStravaAthleteId);
        if (token == null)
            throw new ArgumentNullException(nameof(token), "Invalid token"); // TODO: better handling

        var segments = await _segmentService.GetSegmentsToRefreshAsync(request.SegmentsToRefresh, request.MinimumTimeFromLastRefresh);

        foreach (var segment in segments)
        {
            var getSegmentRes = await _stravaSegmentService.GetSegmentAsync(segment.Id, token);

            if (getSegmentRes.IsSuccess)
            {
                var refreshedSegment = getSegmentRes.Value;

                segment.CreatedAt = refreshedSegment.CreatedAt;
                segment.UpdatedAt = refreshedSegment.UpdatedAt;
                segment.TotalElevationGain = refreshedSegment.TotalElevationGain;
                segment.EffortCount = refreshedSegment.EffortCount;
                segment.AthleteCount = refreshedSegment.AthleteCount;
                segment.StarCount = refreshedSegment.StarCount;
                segment.MapPolyline = refreshedSegment.MapPolyline;
            }
        }

        await _segmentService.UpdateSegmentsAsync(segments);
        await _komUoW.SaveChangesAsync();

        return Result.Ok();
    }

    protected async Task<string?> GetTokenAsync(int athleteId)
    {
        var getValidTokenRes = await _athleteService.GetValidTokenAsync(athleteId);

        return getValidTokenRes.IsSuccess ?
            getValidTokenRes.Value?.AccessToken
            : null;
    }
}
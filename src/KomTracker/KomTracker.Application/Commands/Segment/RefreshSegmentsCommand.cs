using FluentResults;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using MediatR;
using IStravaSegmentService = KomTracker.Application.Interfaces.Services.Strava.ISegmentService;

namespace KomTracker.Application.Commands.Tracking;

public class RefreshSegmentsCommand : IRequest<Result>
{

}

public class RefreshSegmentsCommandHandler : IRequestHandler<RefreshSegmentsCommand, Result>
{
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly ISegmentService _segmentService;
    private readonly IAthleteService _athleteService;
    private readonly IStravaSegmentService _stravaSegmentService;

    public RefreshSegmentsCommandHandler(ApplicationConfiguration applicationConfiguration, ISegmentService segmentService, IAthleteService athleteService, IStravaSegmentService stravaSegmentService)
    {
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaSegmentService = stravaSegmentService ?? throw new ArgumentNullException(nameof(stravaSegmentService));
    }

    public async Task<Result> Handle(RefreshSegmentsCommand request, CancellationToken cancellationToken)
    {
        var masterStravaAthleteId = _applicationConfiguration.MasterStravaAthleteId;
        var segments = await _segmentService.GetSegmentsToRefreshAsync();

        var token = await GetTokenAsync(masterStravaAthleteId);
        if (token == null)
            throw new ArgumentNullException(nameof(token), "Invalid token"); // TODO: better handling

        foreach (var segment in segments)
        {
            var getSegmentRes = await _stravaSegmentService.GetSegmentAsync(segment.Id, token);

            if (getSegmentRes.IsSuccess)
            {
                var refreshedSegment = getSegmentRes.Value;

                segment.AthleteCount = refreshedSegment.AthleteCount;
                segment.EffortCount = refreshedSegment.EffortCount;
            }
        }

        throw new NotImplementedException();
    }

    protected async Task<string?> GetTokenAsync(int athleteId)
    {
        var getValidTokenRes = await _athleteService.GetValidTokenAsync(athleteId);

        return getValidTokenRes.IsSuccess ?
            getValidTokenRes.Value?.AccessToken
            : null;
    }
}
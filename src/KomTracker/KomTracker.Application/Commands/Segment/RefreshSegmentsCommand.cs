using FluentResults;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Services;
using MediatR;

namespace KomTracker.Application.Commands.Tracking;

public class RefreshSegmentsCommand : IRequest<Result>
{

}

public class RefreshSegmentsCommandHandler : IRequestHandler<RefreshSegmentsCommand, Result>
{
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly ISegmentService _segmentService;

    public RefreshSegmentsCommandHandler(ApplicationConfiguration applicationConfiguration, ISegmentService segmentService)
    {
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<Result> Handle(RefreshSegmentsCommand request, CancellationToken cancellationToken)
    {
        var masterStravaAthleteId = _applicationConfiguration.MasterStravaAthleteId;
        var segments = await _segmentService.GetSegmentsToRefreshAsync();

        throw new NotImplementedException();
    }
}
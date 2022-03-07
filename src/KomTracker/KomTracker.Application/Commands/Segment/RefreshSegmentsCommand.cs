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
    private readonly IAthleteService _athleteService;

    public RefreshSegmentsCommandHandler(ApplicationConfiguration applicationConfiguration, ISegmentService segmentService, IAthleteService athleteService)
    {
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<Result> Handle(RefreshSegmentsCommand request, CancellationToken cancellationToken)
    {
        var masterStravaAthleteId = _applicationConfiguration.MasterStravaAthleteId;
        var segments = await _segmentService.GetSegmentsToRefreshAsync();

        var token = await GetTokenAsync(masterStravaAthleteId);
        if (token == null)
            throw new ArgumentNullException(nameof(token), "Invalid token"); // TODO: better handling

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
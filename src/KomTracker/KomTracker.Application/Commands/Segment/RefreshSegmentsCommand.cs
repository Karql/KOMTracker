using FluentResults;
using KomTracker.Application.Services;
using MediatR;

namespace KomTracker.Application.Commands.Tracking;

public class RefreshSegmentsCommand : IRequest<Result>
{

}

public class RefreshSegmentsCommandHandler : IRequestHandler<RefreshSegmentsCommand, Result>
{
    private readonly ISegmentService _segmentService;

    public RefreshSegmentsCommandHandler(ISegmentService segmentService)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<Result> Handle(RefreshSegmentsCommand request, CancellationToken cancellationToken)
    {
        var segments = await _segmentService.GetSegmentsToRefreshAsync();

        throw new NotImplementedException();
    }
}
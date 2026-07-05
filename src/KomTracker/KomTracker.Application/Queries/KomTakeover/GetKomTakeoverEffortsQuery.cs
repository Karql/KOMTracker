using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.KomTakeover;

public class GetKomTakeoverEffortsQuery : IRequest<IEnumerable<EffortModel>>
{
    public int WinnerAthleteId { get; set; }
    public int LoserAthleteId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ActivityType { get; set; }
}

public class GetKomTakeoverEffortsQueryHandler : IRequestHandler<GetKomTakeoverEffortsQuery, IEnumerable<EffortModel>>
{
    private readonly ISegmentService _segmentService;

    public GetKomTakeoverEffortsQueryHandler(ISegmentService segmentService)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<EffortModel>> Handle(GetKomTakeoverEffortsQuery request, CancellationToken cancellationToken)
    {
        // WinnerAthlete took KOMs from LoserAthlete => taken efforts belong to the winner.
        return await _segmentService.GetTakeoverEffortsAsync(
            request.WinnerAthleteId, request.LoserAthleteId, request.DateFrom, request.DateTo, request.ActivityType);
    }
}

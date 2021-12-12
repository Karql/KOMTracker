using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.Athlete;

public class GetAllKomsQuery : IRequest<IEnumerable<SegmentEffortEntity>>
{
    public int AthleteId { get; set; }
}

public class GetAllKomsQueryHandler : IRequestHandler<GetAllKomsQuery, IEnumerable<SegmentEffortEntity>>
{
    private readonly ISegmentService _segmentService;

    public GetAllKomsQueryHandler(ISegmentService segmentService)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<SegmentEffortEntity>> Handle(GetAllKomsQuery request, CancellationToken cancellationToken)
    {
        var lastKomsSummaryEfforts = await _segmentService.GetLastKomsSummaryEffortsAsync(request.AthleteId);

        return lastKomsSummaryEfforts.Select(x => x.SegmentEffort).ToArray();
    }
}

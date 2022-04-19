using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.Athlete;

public class GetAllKomsQuery : IRequest<IEnumerable<EffortModel>>
{
    public int AthleteId { get; set; }
}

public class GetAllKomsQueryHandler : IRequestHandler<GetAllKomsQuery, IEnumerable<EffortModel>>
{
    private readonly ISegmentService _segmentService;

    public GetAllKomsQueryHandler(ISegmentService segmentService)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<EffortModel>> Handle(GetAllKomsQuery request, CancellationToken cancellationToken)
    {
        var lastKomsSummaryEfforts = (await _segmentService.GetLastKomsSummaryEffortsAsync(request.AthleteId))?
            .Where(x => x.SummarySegmentEffort.Kom)
            .ToList()
            ?? Enumerable.Empty<EffortModel>();

        return lastKomsSummaryEfforts;
    }
}

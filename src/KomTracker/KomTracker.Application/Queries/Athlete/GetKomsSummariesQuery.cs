using KomTracker.Application.Models.Segment;
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

public class GetKomsSummariesQuery : IRequest<IEnumerable<KomsSummaryEntity>>
{
    public int AthleteId { get; set; }

    public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(-30).Date.ToUniversalTime();
}

public class GetKomsSummariesQueryHandler : IRequestHandler<GetKomsSummariesQuery, IEnumerable<KomsSummaryEntity>>
{
    private readonly ISegmentService _segmentService;

    public GetKomsSummariesQueryHandler(ISegmentService segmentService)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<KomsSummaryEntity>> Handle(GetKomsSummariesQuery request, CancellationToken cancellationToken)
    {
        var komsSummaries = await _segmentService.GetKomsSummariesAsync(request.AthleteId, request.DateFrom);

        return komsSummaries
            .GroupBy(x => x.TrackDate.Date)
            .Select(x => x.Last())
            .ToList();
    }
}

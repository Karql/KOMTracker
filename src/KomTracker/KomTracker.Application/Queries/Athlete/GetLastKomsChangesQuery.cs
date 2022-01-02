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

public class GetLastKomsChangesQuery : IRequest<IEnumerable<EffortModel>>
{
    public int AthleteId { get; set; }

    public DateTime DateFrom { get; set; } = DateTime.UtcNow.AddDays(-30).Date.ToUniversalTime();
}

public class GetLastKomsChangesQueryHandler : IRequestHandler<GetLastKomsChangesQuery, IEnumerable<EffortModel>>
{
    private readonly ISegmentService _segmentService;

    public GetLastKomsChangesQueryHandler(ISegmentService segmentService)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<EffortModel>> Handle(GetLastKomsChangesQuery request, CancellationToken cancellationToken)
    {
        return await _segmentService.GetLastKomsChangesAsync(request.AthleteId, request.DateFrom);
    }
}

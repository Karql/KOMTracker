using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Application.Shared.Helpers;
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
    private readonly IKOMUnitOfWork _komUoW;

    public GetAllKomsQueryHandler(ISegmentService segmentService, IKOMUnitOfWork komUoW)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<EffortModel>> Handle(GetAllKomsQuery request, CancellationToken cancellationToken)
    {
        var komsEfforts = (await _segmentService.GetLastKomsSummaryEffortsAsync(request.AthleteId))?
            .Where(x => x.SummarySegmentEffort.Kom)
            .ToList()
            ?? new List<EffortModel>();

        // The Burn needs the KOM holder's weight/sex (all these KOMs belong to AthleteId).
        var athlete = await _komUoW.GetRepository<IAthleteRepository>().GetAthleteAsync(request.AthleteId);
        var sex = athlete?.Sex;
        var weight = athlete?.Weight ?? 0f;

        foreach (var effort in komsEfforts)
        {
            KomRatingEnricher.Apply(effort, sex, weight);
        }

        return komsEfforts;
    }
}

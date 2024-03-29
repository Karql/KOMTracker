﻿using KomTracker.Application.Models.Segment;
using KomTracker.Application.Models.Stats;
using KomTracker.Application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.Stats;

public class GetLastKomsChangesQuery : IRequest<IEnumerable<EffortWithAthleteModel>>
{
    public int Top { get; set; } = 100;

    public long? ClubId { get; set; } = null;
}

public class GetLastKomsChangesQueryHandler : IRequestHandler<GetLastKomsChangesQuery, IEnumerable<EffortWithAthleteModel>>
{
    private readonly IAthleteService _athleteService;
    private readonly ISegmentService _segmentService;

    public GetLastKomsChangesQueryHandler(IAthleteService athleteService, ISegmentService segmentService)
    {
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<EffortWithAthleteModel>> Handle(GetLastKomsChangesQuery request, CancellationToken cancellationToken)
    {
        var athletes = (request.ClubId.HasValue) ?
            await _athleteService.GetAthletesByClubAsync(request.ClubId.Value)
            : await _athleteService.GetAllAthletesAsync();

        var changes = await _segmentService.GetLastKomsChangesAsync(athletes.Select(x => x.AthleteId).ToHashSet(), top: request.Top);

        return athletes
            .Join(changes, a => a.AthleteId, c => c.SegmentEffort.AthleteId, (a, c) => new EffortWithAthleteModel(c, a))
            .OrderByDescending(x => x.Effort.SummarySegmentEffort.AuditCD)
            .ToList();
    }
}

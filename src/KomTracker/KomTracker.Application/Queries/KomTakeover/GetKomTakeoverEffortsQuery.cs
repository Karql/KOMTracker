using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
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
    private readonly IKOMUnitOfWork _komUoW;

    public GetKomTakeoverEffortsQueryHandler(ISegmentService segmentService, IKOMUnitOfWork komUoW)
    {
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
    }

    public async Task<IEnumerable<EffortModel>> Handle(GetKomTakeoverEffortsQuery request, CancellationToken cancellationToken)
    {
        // WinnerAthlete took KOMs from LoserAthlete => taken efforts belong to the winner.
        var efforts = (await _segmentService.GetTakeoverEffortsAsync(
            request.WinnerAthleteId, request.LoserAthleteId, request.DateFrom, request.DateTo, request.ActivityType))
            .ToList();

        // All taken efforts belong to the winner, so load their weight/sex once for The Burn.
        var winner = await _komUoW.GetRepository<IAthleteRepository>().GetAthleteAsync(request.WinnerAthleteId);
        var sex = winner?.Sex;
        var weight = winner?.Weight ?? 0f;

        foreach (var effort in efforts)
        {
            KomRatingEnricher.Apply(effort, sex, weight);
        }

        return efforts;
    }
}

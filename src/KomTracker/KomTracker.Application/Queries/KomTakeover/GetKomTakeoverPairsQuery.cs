using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.KomTakeover;

public class GetKomTakeoverPairsQuery : IRequest<IEnumerable<KomTakeoverPairModel>>
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? ActivityType { get; set; }
    public long? ClubId { get; set; }
}

public class GetKomTakeoverPairsQueryHandler : IRequestHandler<GetKomTakeoverPairsQuery, IEnumerable<KomTakeoverPairModel>>
{
    private readonly IAthleteService _athleteService;
    private readonly ISegmentService _segmentService;

    public GetKomTakeoverPairsQueryHandler(IAthleteService athleteService, ISegmentService segmentService)
    {
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<IEnumerable<KomTakeoverPairModel>> Handle(GetKomTakeoverPairsQuery request, CancellationToken cancellationToken)
    {
        var athletes = request.ClubId.HasValue
            ? await _athleteService.GetAthletesByClubAsync(request.ClubId.Value)
            : await _athleteService.GetAllAthletesAsync();

        var athletesById = athletes.ToDictionary(x => x.AthleteId);

        // Club => both sides must be members; no club => all app users (null skips the filter).
        var athleteIdsFilter = request.ClubId.HasValue ? athletesById.Keys.ToHashSet() : null;

        var counts = await _segmentService.GetTakeoverCountsAsync(
            athleteIdsFilter, request.DateFrom, request.DateTo, request.ActivityType);

        return OrientPairs(counts, athletesById);
    }

    /// <summary>
    /// Aggregates directed takeover counts into unordered head-to-head pairs oriented winner-left
    /// (more takeovers wins; tie => lower AthleteId on the left). Ordered by total takeovers desc.
    /// Pure - unit tested.
    /// </summary>
    public static IEnumerable<KomTakeoverPairModel> OrientPairs(
        IEnumerable<KomTakeoverCountModel> counts,
        IReadOnlyDictionary<int, AthleteEntity> athletesById)
    {
        var pairs = new Dictionary<(int LowId, int HighId), (int LowToHigh, int HighToLow)>();

        foreach (var c in counts)
        {
            if (c.TakenByAthleteId == c.LostByAthleteId) continue; // safety

            var lowId = Math.Min(c.TakenByAthleteId, c.LostByAthleteId);
            var highId = Math.Max(c.TakenByAthleteId, c.LostByAthleteId);
            var key = (lowId, highId);

            pairs.TryGetValue(key, out var v);
            if (c.TakenByAthleteId == lowId) v.LowToHigh += c.Count;
            else v.HighToLow += c.Count;
            pairs[key] = v;
        }

        var result = new List<KomTakeoverPairModel>();

        foreach (var (key, v) in pairs)
        {
            // Winner = more takeovers; tie => lower id (LowId) on the left.
            var lowIsWinner = v.LowToHigh >= v.HighToLow;

            var winnerId = lowIsWinner ? key.LowId : key.HighId;
            var loserId = lowIsWinner ? key.HighId : key.LowId;
            var winnerKoms = lowIsWinner ? v.LowToHigh : v.HighToLow;
            var loserKoms = lowIsWinner ? v.HighToLow : v.LowToHigh;

            if (!athletesById.TryGetValue(winnerId, out var winner) || !athletesById.TryGetValue(loserId, out var loser))
                continue; // athlete out of scope (shouldn't happen given filtering)

            result.Add(new KomTakeoverPairModel
            {
                WinnerAthlete = winner,
                WinnerKoms = winnerKoms,
                LoserAthlete = loser,
                LoserKoms = loserKoms
            });
        }

        return result
            .OrderByDescending(x => x.WinnerKoms + x.LoserKoms)
            .ThenByDescending(x => x.WinnerKoms)
            .ToList();
    }
}

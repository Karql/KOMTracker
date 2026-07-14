using KomTracker.Application.Models.Segment;
using KomTracker.Application.Models.Stats;
using KomTracker.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utils.Extensions;

namespace KomTracker.Application.Queries.Ranking;

public enum KomsChangesPeriod
{
    ThisWeek,
    LastWeek,
    Last30Days
}

public enum KomsChangeDirection
{
    New,
    Lost
}

/// <summary>
/// The New/Lost KOMs behind a single ranking "Koms changes" cell (athlete + period + direction),
/// read from the precomputed athlete stats. Powers the Ranking click-to-open modal.
/// </summary>
public class GetKomsChangesDetailsQuery : IRequest<IEnumerable<EffortModel>>
{
    public int AthleteId { get; set; }
    public KomsChangesPeriod Period { get; set; }
    public KomsChangeDirection Direction { get; set; }
    public string? ActivityType { get; set; }
}

public class GetKomsChangesDetailsQueryHandler : IRequestHandler<GetKomsChangesDetailsQuery, IEnumerable<EffortModel>>
{
    private readonly ILogger _logger;
    private readonly IAthleteService _athleteService;

    public GetKomsChangesDetailsQueryHandler(ILogger<GetKomsChangesDetailsQueryHandler> logger, IAthleteService athleteService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    public async Task<IEnumerable<EffortModel>> Handle(GetKomsChangesDetailsQuery request, CancellationToken cancellationToken)
    {
        var statsEntities = await _athleteService.GetAthletesStatsAsync(new HashSet<int> { request.AthleteId });
        var entity = statsEntities.FirstOrDefault();

        if (entity == null)
        {
            return Enumerable.Empty<EffortModel>();
        }

        var stats = JsonSerializer.Deserialize<AthleteStatsModel>(entity.StatsJson);

        if (stats == null)
        {
            _logger.LogWarning("Cannot deserialize AthleteStatsModel for {athleteId}", entity.AthleteId);
            return Enumerable.Empty<EffortModel>();
        }

        var window = request.Period switch
        {
            KomsChangesPeriod.ThisWeek => stats.KomsChangesThisWeek,
            KomsChangesPeriod.LastWeek => stats.KomsChangesLastWeek,
            _ => stats.KomsChangesLast30Days,
        };

        var koms = request.Direction == KomsChangeDirection.New ? window.NewKoms : window.LostKoms;

        var efforts = koms
            .Where(x => string.IsNullOrWhiteSpace(request.ActivityType)
                || x.Segment!.ActivityType.EqualsCI(request.ActivityType))
            .ToList();

        foreach (var effort in efforts)
        {
            KomRatingEnricher.Apply(effort, stats.Athlete.Sex, stats.Athlete.Weight);
        }

        return efforts;
    }
}

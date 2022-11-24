using KomTracker.Application.Models.Ranking;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Models.Stats;
using KomTracker.Application.Services;
using KomTracker.Application.Shared.Helpers;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace KomTracker.Application.Queries.Ranking;

public class GetRankingQuery : IRequest<IEnumerable<AthleteRankingModel>>
{
    public long? ClubId { get; set; } = null;
}

public class GetRankingQueryHandler : IRequestHandler<GetRankingQuery, IEnumerable<AthleteRankingModel>>
{
    private readonly ILogger _logger;
    private readonly IAthleteService _athleteService;

    public GetRankingQueryHandler(ILogger<GetRankingQueryHandler> logger, IAthleteService athleteService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    public async Task<IEnumerable<AthleteRankingModel>> Handle(GetRankingQuery request, CancellationToken cancellationToken)
    {
        var athletes = (request.ClubId.HasValue) ?
            await _athleteService.GetAthletesByClubAsync(request.ClubId.Value)
            : await _athleteService.GetAllAthletesAsync();

        var athleteStatsEntities = await _athleteService.GetAthletesStatsAsync(athletes.Select(x => x.AthleteId).ToHashSet());

        return GetRankingFromEntities(athleteStatsEntities, request)
            .OrderByDescending(x => x.Total.KomsCount)
            .ToArray();
    }

    public IEnumerable<AthleteRankingModel> GetRankingFromEntities(IEnumerable<Domain.Entities.Athlete.AthleteStatsEntity> athleteStatsEntities, GetRankingQuery request)
    {
        foreach (var e in athleteStatsEntities)
        {
            var athleteStats = JsonSerializer.Deserialize<AthleteStatsModel>(e.StatsJson);

            if (athleteStats == null)
            {
                _logger.LogWarning("Cannot deserialize AthleteStatsModel for {athleteId}", e.AthleteId);
                continue;
            }

            yield return GetAthleteRankingModel(athleteStats, request);
        }
    }

    public AthleteRankingModel GetAthleteRankingModel(AthleteStatsModel athleteStats, GetRankingQuery request)
    {
        var model = new AthleteRankingModel
        {
            Athlete = athleteStats.Athlete,
        };

        var koms = athleteStats.Koms.ToArray(); // TOOD: filter by request activity type

        model.Total.KomsCount = koms.Count();

        model.Total.KomsCountByCategory = koms
            .GroupBy(x => SegmentHelper.GetExtendedCategory(x.Segment!.ClimbCategory, x.Segment.AverageGrade, x.Segment.Distance))
            .ToDictionary(x => x.Key, v => v.Count());

        return model;
    }
}

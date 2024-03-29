﻿using KomTracker.Application.Models.Ranking;
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
using Utils.Extensions;

namespace KomTracker.Application.Queries.Ranking;

public class GetRankingQuery : IRequest<IEnumerable<AthleteRankingModel>>
{
    public long? ClubId { get; set; } = null;
    public string? ActivityType { get; set; }
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

    private AthleteRankingModel GetAthleteRankingModel(AthleteStatsModel athleteStats, GetRankingQuery request)
    {
        return new AthleteRankingModel
        {
            Athlete = athleteStats.Athlete,
            Total = GetAthleteRankingTotalModel(athleteStats.Koms, request),
            KomsChanges30Days = GetAthleteRankingKomsChangesModel(athleteStats.KomsChangesLast30Days, request),
            KomsChangesLastWeek = GetAthleteRankingKomsChangesModel(athleteStats.KomsChangesLastWeek, request),
            KomsChangesThisWeek = GetAthleteRankingKomsChangesModel(athleteStats.KomsChangesThisWeek, request)
        };
    }

    private AthleteRankingTotalModel GetAthleteRankingTotalModel(IEnumerable<EffortModel> koms, GetRankingQuery request)
    {
        koms = koms
            .Where(x => string.IsNullOrWhiteSpace(request.ActivityType) 
                || x.Segment!.ActivityType.EqualsCI(request.ActivityType))
            .ToArray();

        return new AthleteRankingTotalModel
        {
            KomsCount = koms.Count(),
            KomsCountByCategory = koms
                .GroupBy(x => SegmentHelper.GetExtendedCategory(x.Segment!.ClimbCategory, x.Segment.AverageGrade, x.Segment.Distance))
                .ToDictionary(x => x.Key, v => v.Count())
        };
    }

    private AthleteRankingKomsChangesModel GetAthleteRankingKomsChangesModel(KomsChangesModel komsChanges, GetRankingQuery request)
    {
        var newKomsCount = komsChanges
            .NewKoms
            .Where(x => string.IsNullOrWhiteSpace(request.ActivityType)
                || x.Segment!.ActivityType.EqualsCI(request.ActivityType))
            .Count();

        var lostKomsCount = komsChanges
            .LostKoms
            .Where(x => string.IsNullOrWhiteSpace(request.ActivityType)
                || x.Segment!.ActivityType.EqualsCI(request.ActivityType))
            .Count();

        return new AthleteRankingKomsChangesModel
        {
            NewKomsCount = newKomsCount,
            LostKomsCount = lostKomsCount,
        };               
    }
}

﻿using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;

namespace KomTracker.Application.Services;

public class SegmentService : ISegmentService
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<SegmentService> _logger;
    private readonly IStravaAthleteService _stravaAthleteService;

    public SegmentService(IKOMUnitOfWork komUoW, ILogger<SegmentService> logger, IStravaAthleteService stravaAthleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _stravaAthleteService = stravaAthleteService ?? throw new ArgumentNullException(nameof(stravaAthleteService));
    }

    public async Task<IEnumerable<(SegmentEffortEntity, SegmentEntity)>> GetActualKomsAsync(int athleteId, string token)
    {
        var getAthleteKomsRes = await _stravaAthleteService.GetAthleteKomsAsync(athleteId, token);

        // TODO: retry on Unauthorized
        return getAthleteKomsRes.IsSuccess ?
            getAthleteKomsRes.Value
            : null;
    }

    public async Task<IEnumerable<SegmentEffortWithLinkToKomsSummaryModel>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        return (await _komUoW
            .GetRepository<ISegmentRepository>()
            .GetLastKomsSummaryEffortsWithLinksAsync(athleteId))
            ?? Enumerable.Empty<SegmentEffortWithLinkToKomsSummaryModel>();
    }

    public ComparedEffortsModel CompareEfforts(IEnumerable<SegmentEffortEntity> actualKomsEfforts, IEnumerable<SegmentEffortEntity> lastKomsEfforts)
    {
        var comparedEfforts = new ComparedEffortsModel();

        actualKomsEfforts.FullGroupJoin(lastKomsEfforts,
            x => x.SegmentId,
            x => x.SegmentId,
            (key, newEfforts, lastEfforts) => new { NewEffort = newEfforts.FirstOrDefault(), LastEffort = lastEfforts.FirstOrDefault() }
        ).ForEach(x =>
        {
            SegmentEffortEntity effort = x.NewEffort ?? x.LastEffort;           
            KomsSummarySegmentEffortEntity link = new()
            { 
                SegmentEffortId = effort.Id // by id to prevent add effort
            };

            if (x.NewEffort != null)
            {
                comparedEfforts.KomsCount++;
                link.Kom = true;

                if (x.LastEffort == null)
                {
                    comparedEfforts.NewKomsCount++;
                    link.NewKom = true;
                }

                else if (x.NewEffort.Id != x.LastEffort.Id)
                {
                    comparedEfforts.ImprovedKomsCount++;
                    link.ImprovedKom = true;
                }
            }

            else
            {
                comparedEfforts.LostKomsCount++;
                link.LostKom = true;
            }

            comparedEfforts.EffortsWithLinks.Add(new SegmentEffortWithLinkToKomsSummaryModel
            {
                SegmentEffort = effort,
                Link = link
            });
        });

        return comparedEfforts;
    }

    public async Task AddSegmentsIfNotExistsAsync(IEnumerable<SegmentEntity> segments)
    {
        await _komUoW.GetRepository<ISegmentRepository>()
            .AddSegmentsIfNotExistsAsync(segments);
    }

    public async Task AddSegmentEffortsIfNotExistsAsync(IEnumerable<SegmentEffortEntity> segmentEfforts)
    {
        await _komUoW.GetRepository<ISegmentRepository>()
            .AddSegmentEffortsIfNotExistsAsync(segmentEfforts);
    }

    public async Task AddNewKomsSummaryWithEffortsAsync(int athleteId, ComparedEffortsModel comparedEfforts)
    {
        var segmentRepo = _komUoW.GetRepository<ISegmentRepository>();

        var komsSummary = new KomsSummaryEntity
        {
            AthleteId = athleteId,
            TrackDate = DateTime.UtcNow,
            Koms = comparedEfforts.KomsCount,
            NewKoms = comparedEfforts.NewKomsCount,
            ImprovedKoms = comparedEfforts.ImprovedKomsCount,
            LostKoms = comparedEfforts.LostKomsCount,
        };

        await segmentRepo.AddKomsSummaryAsync(komsSummary);

        var komsSummariesSegmentEfforts = comparedEfforts.EffortsWithLinks.Select(x => x.Link);
        komsSummariesSegmentEfforts.ForEach(x => x.KomsSummary = komsSummary);
        await segmentRepo.AddKomsSummariesSegmentEffortsAsync(komsSummariesSegmentEfforts);
    }
}
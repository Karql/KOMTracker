using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
using static MoreLinq.Extensions.FullGroupJoinExtension;
using static MoreLinq.Extensions.ForEachExtension;

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

    public async Task<IEnumerable<EffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        return await _komUoW
            .GetRepository<ISegmentRepository>()
            .GetLastKomsSummaryEffortsAsync(athleteId);
    }

    public Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(int athleteId, DateTime dateFrom)
    {
        return GetLastKomsChangesAsync(new[] { athleteId }, dateFrom: dateFrom);
    }

    public async Task<IEnumerable<EffortModel>> GetLastKomsChangesAsync(IEnumerable<int> athleteIds, DateTime? dateFrom = null, int? top = null)
    {
        return (await _komUoW
            .GetRepository<ISegmentRepository>()
            .GetLastKomsChangesAsync(athleteIds, dateFrom, top))
            ?? Enumerable.Empty<EffortModel>();
    }

    public async Task<IEnumerable<KomsSummaryEntity>> GetKomsSummariesAsync(int athleteId, DateTime dateFrom)
    {
        return (await _komUoW
            .GetRepository<ISegmentRepository>()
            .GetKomsSummariesAsync(athleteId, dateFrom))
            ?? Enumerable.Empty<KomsSummaryEntity>();
    }

    // TODO: CompareEfforts not assigns segment
    public ComparedEffortsModel CompareEfforts(IEnumerable<SegmentEffortEntity> actualKomsEfforts, IEnumerable<SegmentEffortEntity> lastKomsEfforts, bool firstCompare = false)
    {
        var comparedEfforts = new ComparedEffortsModel
        {
            FirstCompare = firstCompare
        };

        actualKomsEfforts.FullGroupJoin(lastKomsEfforts,
            x => x.SegmentId,
            x => x.SegmentId,
            (key, newEfforts, lastEfforts) => new { NewEffort = newEfforts.FirstOrDefault(), LastEffort = lastEfforts.FirstOrDefault() }
        ).ForEach(x =>
        {
            SegmentEffortEntity effort = x.NewEffort ?? x.LastEffort!;           
            KomsSummarySegmentEffortEntity link = new()
            { 
                SegmentEffortId = effort.Id // by id to prevent add effort
            };

            if (x.NewEffort != null)
            {
                comparedEfforts.KomsCount++;
                link.Kom = true;

                // don't mark as new in first comparing
                if (!firstCompare) 
                {
                    if (x.LastEffort == null)
                    {
                        comparedEfforts.NewKomsCount++;
                        link.NewKom = true;
                    }

                    else if (x.NewEffort.Id != x.LastEffort.Id
                        // same effort with diffrent id
                        // this situation occurs in actions like:
                        // - refresh activity achievements
                        // - change privacy zone
                        // current segment_effort is deleted and a new one is created
                        && x.NewEffort.ElapsedTime < x.LastEffort.ElapsedTime)
                    {
                        comparedEfforts.ImprovedKomsCount++;
                        link.ImprovedKom = true;
                    }
                }
            }

            else
            {
                comparedEfforts.LostKomsCount++;
                link.LostKom = true;
            }

            comparedEfforts.Efforts.Add(new EffortModel
            {
                SegmentEffort = effort,
                SummarySegmentEffort = link
            });
        });

        comparedEfforts.PreviousKomsCount = lastKomsEfforts.Count();

        return comparedEfforts;
    }

    public async Task CheckNewKomsAreReturnedAsync(ComparedEffortsModel comparedEfforts)
    {
        var newKomsEfforts = comparedEfforts.Efforts.Where(x => x.SummarySegmentEffort.NewKom).ToArray();

        var segmentEfforts = await _komUoW.GetRepository<ISegmentRepository>()
            .GetSegmentEffortsAsync(newKomsEfforts.Select(x => x.SegmentEffort.Id).ToHashSet());

        newKomsEfforts
            .Join(segmentEfforts, x => x.SegmentEffort.Id, y => y.Id, (effort, segmentEffort) => effort)
            .ForEach(effort =>
            {
                effort.SummarySegmentEffort.NewKom = false;
                effort.SummarySegmentEffort.ReturnedKom = true;
                comparedEfforts.NewKomsCount--;
                comparedEfforts.ReturnedKomsCount++;
            });
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

    public async Task<KomsSummaryEntity> AddNewKomsSummaryWithEffortsAsync(int athleteId, ComparedEffortsModel comparedEfforts)
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
            ReturnedKoms = comparedEfforts.ReturnedKomsCount
        };

        await segmentRepo.AddKomsSummaryAsync(komsSummary);

        var komsSummariesSegmentEfforts = comparedEfforts.Efforts.Select(x => x.SummarySegmentEffort);
        komsSummariesSegmentEfforts.ForEach(x => x.KomsSummary = komsSummary);
        await segmentRepo.AddKomsSummariesSegmentEffortsAsync(komsSummariesSegmentEfforts);

        return komsSummary;
    }

    public async Task<IEnumerable<SegmentEntity>> GetSegmentsToRefreshAsync(int top = 100, TimeSpan? minTimeFromLastRefresh = null)
    {
        return await _komUoW.GetRepository<ISegmentRepository>()
            .GetSegmentsToRefreshAsync(top, minTimeFromLastRefresh);
    }

    public async Task UpdateSegmentsAsync(IEnumerable<SegmentEntity> segments)
    {
        await _komUoW.GetRepository<ISegmentRepository>()
            .UpdateSegmentsAsync(segments);
    }

    public ResolveTakeoversResult ResolveTakeovers(
        IEnumerable<KomTakeoverChangeModel> summaryChanges,
        IEnumerable<KomTakeoverChangeModel> counterpartChanges,
        IEnumerable<KomTakeoverEntity> activeTakeoversByLostEffort,
        ISet<long> existingTakenSegmentEffortIds)
    {
        var result = new ResolveTakeoversResult();

        var counterparts = counterpartChanges?.ToList() ?? new List<KomTakeoverChangeModel>();
        var activeTakeovers = activeTakeoversByLostEffort?.ToList() ?? new List<KomTakeoverEntity>();
        var existingTaken = existingTakenSegmentEffortIds ?? new HashSet<long>();

        // Track winning efforts already produced in this run too (avoid in-batch dups).
        var producedTakenEffortIds = new HashSet<long>();

        foreach (var change in summaryChanges ?? Enumerable.Empty<KomTakeoverChangeModel>())
        {
            switch (change.ChangeType)
            {
                case KomChangeTypeEnum.New:
                    TryCreateTakeover(
                        winner: change,
                        loser: FindNewestCounterpart(counterparts, change, KomChangeTypeEnum.Lost),
                        result, existingTaken, producedTakenEffortIds);
                    break;

                case KomChangeTypeEnum.Lost:
                    TryCreateTakeover(
                        winner: FindNewestCounterpart(counterparts, change, KomChangeTypeEnum.New),
                        loser: change,
                        result, existingTaken, producedTakenEffortIds);
                    break;

                case KomChangeTypeEnum.Returned:
                    var takeover = activeTakeovers
                        .Where(x => x.LostSegmentEffortId == change.SegmentEffortId)
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();

                    if (takeover != null && !result.RevertedTakeoverIds.Contains(takeover.Id))
                    {
                        result.RevertedTakeoverIds.Add(takeover.Id);
                    }
                    break;
            }
        }

        return result;
    }

    private static KomTakeoverChangeModel? FindNewestCounterpart(
        IEnumerable<KomTakeoverChangeModel> counterparts, KomTakeoverChangeModel change, KomChangeTypeEnum counterpartType)
    {
        return counterparts
            .Where(x => x.ChangeType == counterpartType
                && x.SegmentId == change.SegmentId
                && x.AthleteId != change.AthleteId
                && SexEquals(x.Sex, change.Sex))
            .OrderByDescending(x => x.KomsSummaryId)
            .FirstOrDefault();
    }

    private static void TryCreateTakeover(
        KomTakeoverChangeModel? winner, KomTakeoverChangeModel? loser,
        ResolveTakeoversResult result, ISet<long> existingTaken, ISet<long> producedTaken)
    {
        if (winner == null || loser == null)
        {
            return;
        }

        if (existingTaken.Contains(winner.SegmentEffortId) || !producedTaken.Add(winner.SegmentEffortId))
        {
            return; // already recorded or already produced in this run
        }

        result.NewTakeovers.Add(new KomTakeoverEntity
        {
            TakenSegmentEffortId = winner.SegmentEffortId,
            LostSegmentEffortId = loser.SegmentEffortId,
            TakenKomsSummaryId = winner.KomsSummaryId,
            LostKomsSummaryId = loser.KomsSummaryId,
            TrackDate = winner.TrackDate,
            Reverted = false
        });
    }

    private static bool SexEquals(string? a, string? b)
    {
        // Exact match; null == null counts as equal.
        return string.Equals(a, b, StringComparison.Ordinal);
    }
}

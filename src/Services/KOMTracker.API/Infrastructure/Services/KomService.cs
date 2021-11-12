using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Models.Segment;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Infrastructure.Services;

public class KomService : IKomService
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<KomService> _logger;
    private readonly IAthleteService _athleteService;

    public KomService(IKOMUnitOfWork komUoW, ILogger<KomService> logger, IAthleteService athleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    public async Task TrackKomsAsync()
    {
        var athlets = await _athleteService.GetAllAthletesAsync();

        foreach (var athlete in athlets)
        {
            await TrackKomsForAthleteAsync(athlete.AthleteId);
        }
    }

    protected async Task TrackKomsForAthleteAsync(int athleteId)
    {
        var token = await GetTokenAsync(athleteId);
        if (token == null) return;

        var actualKoms = await GetActualKomsAsync(athleteId, token);
        if (actualKoms == null) return;

        var lastEfforts = await GetLastKomsSummaryEffortsAsync(athleteId);

        var comparedEfforts = CompareEfforts(actualKoms.Select(x => x.Item1), lastEfforts);

        if (comparedEfforts.AnyChanges)
        {
            // TODO: transaction
            await AddSegmentsIfNotExists(actualKoms.Select(x => x.Item2));
            await AddNewKomsSummaryAsync(athleteId, comparedEfforts);

            await _komUoW.SaveChangesAsync();
        }
    }

    protected async Task<string> GetTokenAsync(int athleteId)
    {
        var getValidTokenRes = await _athleteService.GetValidTokenAsync(athleteId);

        return getValidTokenRes.IsSuccess ? 
            getValidTokenRes.Value?.AccessToken
            : null;
    }

    protected async Task<IEnumerable<(SegmentEffortModel, SegmentModel)>> GetActualKomsAsync(int athleteId, string token)
    {
        var getAthleteKomsRes = await _athleteService.GetAthleteKomsAsync(athleteId, token);

        // TODO: retry on Unauthorized
        return getAthleteKomsRes.IsSuccess ?
            getAthleteKomsRes.Value
            : null;
    }

    protected async Task<IEnumerable<SegmentEffortModel>> GetLastKomsSummaryEffortsAsync(int athleteId)
    {
        return (await _komUoW
            .GetRepository<ISegmentRepository>()
            .GetLastKomsSummaryEffortsAsync(athleteId))
            ?? Enumerable.Empty<SegmentEffortModel>();
    }

    protected ComparedEffortsModel CompareEfforts(IEnumerable<SegmentEffortModel> actualEfforts, IEnumerable<SegmentEffortModel> lastEfforts)
    {
        var comparedEfforts = new ComparedEffortsModel();

        actualEfforts.FullGroupJoin(lastEfforts,
            x => x.SegmentId,
            x => x.SegmentId,
            (key, newEfforts, lastEfforts) => new { NewEffort = newEfforts.FirstOrDefault(), LastEffort = lastEfforts.FirstOrDefault() }
        ).ForEach(x =>
        {
            if (x.NewEffort != null)
            {
                comparedEfforts.Koms.Add(x.NewEffort);

                if (x.LastEffort == null)
                {
                    comparedEfforts.NewKoms.Add(x.NewEffort);
                }

                else if (x.NewEffort.SegmentId != x.LastEffort.SegmentId)
                {
                    comparedEfforts.ImprovedKoms.Add(x.NewEffort);
                }
            }

            else
            {
                comparedEfforts.LostKoms.Add(x.LastEffort);
            }
        });

        return comparedEfforts;
    }

    protected async Task AddSegmentsIfNotExists(IEnumerable<SegmentModel> segments)
    {
        await _komUoW.GetRepository<ISegmentRepository>()
            .AddSegmentsIfNotExists(segments);
    }

    protected async Task AddNewKomsSummaryAsync(int athleteId, ComparedEffortsModel comparedEfforts)
    {
        var komsSummary = new KomsSummaryModel
        {
            AthleteId = athleteId,
            TrackDate = DateTime.UtcNow,
            Koms = comparedEfforts.Koms.Count,
            NewKoms = comparedEfforts.NewKoms.Count,
            ImprovedKoms = comparedEfforts.ImprovedKoms.Count,
            LostKoms = comparedEfforts.LostKoms.Count,
            SegmentEfforts = comparedEfforts.Koms
        };

        await _komUoW.GetRepository<ISegmentRepository>()
            .AddKomsSummaryWithEffortsAsync(komsSummary);
    }
}

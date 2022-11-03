using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils.Extensions;

namespace KomTracker.Application.Commands.Stats;
public class RecalculateStatsCommand : IRequest<Result>
{

}

public class RecalculateStatsCommandHandler : IRequestHandler<RecalculateStatsCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<RecalculateStatsCommandHandler> _logger;
    private readonly IAthleteService _athleteService;
    private readonly ISegmentService _segmentService;

    public RecalculateStatsCommandHandler(IKOMUnitOfWork komUoW, ILogger<RecalculateStatsCommandHandler> logger, IAthleteService athleteService, ISegmentService segmentService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<Result> Handle(RecalculateStatsCommand request, CancellationToken cancellationToken)
    {
        var athlets = await _athleteService.GetAllAthletesAsync();

        //foreach (var athlete in athlets)
        foreach (var athlete in athlets.Where(x => x.AthleteId == 2394302))
        {
            if (cancellationToken.IsCancellationRequested) return Result.Ok(); // TODO: OK?

            await RecalculateStatsForAthleteAsync(athlete);
        }

        return Result.Ok();
    }

    private async Task RecalculateStatsForAthleteAsync(AthleteEntity athlete)
    {
        await RecalculateTotalForAthleteAsync(athlete);
        await RecaluclateLastKomsChangesSummaryForAthleteAsync(athlete);
    }

    private async Task RecalculateTotalForAthleteAsync(AthleteEntity athlete)
    {
        var komsEfforts = (await _segmentService.GetLastKomsSummaryEffortsAsync(athlete.AthleteId))?
            .Where(x => x.SummarySegmentEffort.Kom)
            .ToList()
            ?? Enumerable.Empty<EffortModel>();

        GetTotals(komsEfforts);
    }

    private async Task RecaluclateLastKomsChangesSummaryForAthleteAsync(AthleteEntity athlete)
    {
        var last30days = DateTime.UtcNow.AddDays(-30).BeginningOfDay();
        var lastKomChanges = await _segmentService.GetLastKomsChangesAsync(athlete.AthleteId, last30days);

        var now = DateTime.UtcNow;
        var beginningOfWeek = now.BeginningOfWeek();
        var beginningOfLastWeek = beginningOfWeek.AddDays(-1).BeginningOfWeek();
        var endOfLastWeek = beginningOfWeek.AddDays(-1).EndOfWeek();

        var last30daysKomsChangesSummary = GetLastKomsChangesSummary(lastKomChanges, last30days, now);
        var lastWeekKomsChangesSummary = GetLastKomsChangesSummary(lastKomChanges, beginningOfLastWeek, endOfLastWeek);
        var thisWeekKomsChangesSummary = GetLastKomsChangesSummary(lastKomChanges, beginningOfWeek, now);
    }

    private void GetTotals(IEnumerable<EffortModel> komsEfforts)
    {
        var komsCount = komsEfforts.Count();

        var totalDistance = komsEfforts.Sum(x => x.Segment.Distance);
    }

    private (int lostKomsCount, int newKomsCount) GetLastKomsChangesSummary(IEnumerable<EffortModel> lastKomChanges, DateTime dateFrom, DateTime dateTo)
    {
        var lostKomsChanges = new List<EffortModel>();
        var newKomsChanges = new List<EffortModel>();

        // calculate new and lost koms
        foreach (var segmentChanges in lastKomChanges.Where(x => 
            x.SummarySegmentEffort.AuditCD >= dateFrom 
            && x.SummarySegmentEffort.AuditCD <= dateTo).GroupBy(x => x.Segment.Id))
        {
            var lastChange = segmentChanges.OrderByDescending(e => e.SummarySegmentEffort.KomSummaryId).First();
            var anyNewInChanges = segmentChanges.Any(e => e.SummarySegmentEffort.NewKom);

            if (lastChange.SummarySegmentEffort.LostKom)
            {
                lostKomsChanges.Add(lastChange);
            }

            else if (lastChange.SummarySegmentEffort.NewKom
                || (anyNewInChanges && (lastChange.SummarySegmentEffort.ImprovedKom || lastChange.SummarySegmentEffort.ReturnedKom)))
            {
                newKomsChanges.Add(lastChange);
            }
        }

        return (lostKomsChanges.Count, newKomsChanges.Count);
    }
}
using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Models.Stats;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utils.Extensions;

namespace KomTracker.Application.Commands.Stats;
public class RefreshStatsCommand : IRequest<Result>
{
    public int? AthleteId { get; set; }
}

public class RefreshStatsCommandHandler : IRequestHandler<RefreshStatsCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<RefreshStatsCommandHandler> _logger;
    private readonly IAthleteService _athleteService;
    private readonly ISegmentService _segmentService;

    public RefreshStatsCommandHandler(IKOMUnitOfWork komUoW, ILogger<RefreshStatsCommandHandler> logger, IAthleteService athleteService, ISegmentService segmentService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
    }

    public async Task<Result> Handle(RefreshStatsCommand request, CancellationToken cancellationToken)
    {
        var athlets = request.AthleteId.HasValue ?
            new [] { await _athleteService.GetAthleteAsync(request.AthleteId.Value) }
            : await _athleteService.GetAllAthletesAsync();

        foreach (var athlete in athlets)
        {
            if (cancellationToken.IsCancellationRequested) return Result.Ok(); // TODO: OK?

            await RefreshAthleteStatsAsync(athlete);
        }

        return Result.Ok();
    }

    private async Task RefreshAthleteStatsAsync(AthleteEntity athlete)
    {
        var athleteStats = await GetAthleteStatsAsync(athlete);
    }

    private async Task<AthleteStatsModel> GetAthleteStatsAsync(AthleteEntity athlete)
    {
        var koms = await GetAllKomsAsync(athlete);
        var komsChanges = await GetKomsChangesForAthleteAsync(athlete);

        return new AthleteStatsModel(athlete, koms,
            komsChanges.KomsChangesLast30Days, komsChanges.KomsChangesLastWeek, komsChanges.KomsChangesThisWeek);
    }

    private async Task<IEnumerable<EffortModel>> GetAllKomsAsync(AthleteEntity athlete)
    {
        return (await _segmentService.GetLastKomsSummaryEffortsAsync(athlete.AthleteId))?
            .Where(x => x.SummarySegmentEffort.Kom)
            .ToList()
            ?? Enumerable.Empty<EffortModel>();
    }

    private async Task<(KomsChangesModel KomsChangesLast30Days, KomsChangesModel KomsChangesLastWeek, KomsChangesModel KomsChangesThisWeek)> GetKomsChangesForAthleteAsync(AthleteEntity athlete)
    {
        var last30days = DateTime.UtcNow.AddDays(-30).BeginningOfDay();
        var lastKomChanges = await _segmentService.GetLastKomsChangesAsync(athlete.AthleteId, last30days);

        var now = DateTime.UtcNow;
        var beginningOfWeek = now.BeginningOfWeek();
        var beginningOfLastWeek = beginningOfWeek.AddDays(-1).BeginningOfWeek();
        var endOfLastWeek = beginningOfWeek.AddDays(-1).EndOfWeek();

        var komsChangesLast30Days = GetKomsChanges(lastKomChanges, last30days, now);
        var komsChangesLastWeek = GetKomsChanges(lastKomChanges, beginningOfLastWeek, endOfLastWeek);
        var komsChangesThisWeek = GetKomsChanges(lastKomChanges, beginningOfWeek, now);

        return (komsChangesLast30Days, komsChangesLastWeek, komsChangesThisWeek);
    }

    private KomsChangesModel GetKomsChanges(IEnumerable<EffortModel> lastKomChanges, DateTime dateFrom, DateTime dateTo)
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

        return new KomsChangesModel { NewKoms = newKomsChanges, LostKoms = lostKomsChanges };
    }
}
using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Segment;
using MediatR;
using Microsoft.Extensions.Logging;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;

namespace KomTracker.Application.Commands.Tracking;

public class TrackKomsCommand : IRequest<Result>
{

}

public class TrackKomsCommandHandler : IRequestHandler<TrackKomsCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<TrackKomsCommandHandler> _logger;
    private readonly ISegmentService _segmentService;
    private readonly IAthleteService _athleteService;
    private readonly IStravaAthleteService _stravaAthleteService;

    public TrackKomsCommandHandler(IKOMUnitOfWork komUoW, ILogger<TrackKomsCommandHandler> logger, ISegmentService segmentService, IAthleteService athleteService, IStravaAthleteService stravaAthleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaAthleteService = stravaAthleteService ?? throw new ArgumentNullException(nameof(stravaAthleteService));
    }

    public async Task<Result> Handle(TrackKomsCommand request, CancellationToken cancellationToken)
    {
        var athlets = await _athleteService.GetAllAthletesAsync();

        foreach (var athlete in athlets)
        {
            if (cancellationToken.IsCancellationRequested) return Result.Ok(); // TODO: OK?

            await TrackKomsForAthleteAsync(athlete.AthleteId);
        }

        return Result.Ok();
    }

    protected async Task TrackKomsForAthleteAsync(int athleteId)
    {
        var token = await GetTokenAsync(athleteId);
        if (token == null) return;

        var actualKoms = await _segmentService.GetActualKomsAsync(athleteId, token);
        if (actualKoms == null) return;

        var lastKomsSummaryEfforts = await _segmentService.GetLastKomsSummaryEffortsAsync(athleteId);
        var lastKomsEfforts = lastKomsSummaryEfforts.Where(x => x.SummarySegmentEffort.Kom).Select(x => x.SegmentEffort);

        var comparedEfforts = _segmentService.CompareEfforts(actualKoms.Select(x => x.Item1), lastKomsEfforts);

        if (comparedEfforts.AnyChanges)
        {
            var newKomsEfforts = comparedEfforts
                .Efforts
                .Where(x => x.SummarySegmentEffort.NewKom
                    || x.SummarySegmentEffort.ImprovedKom)
                .Select(x => x.SegmentEffort);

            // TODO: transaction
            await _segmentService.AddSegmentsIfNotExistsAsync(actualKoms.Select(x => x.Item2));
            await _segmentService.AddSegmentEffortsIfNotExistsAsync(newKomsEfforts);
            await _segmentService.AddNewKomsSummaryWithEffortsAsync(athleteId, comparedEfforts);

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
}

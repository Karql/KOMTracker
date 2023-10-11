using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Segment;
using KomTracker.Application.Notifications.Tracking;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
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
    private const int MAX_RETRY_COUNT = 5;

    private readonly IMediator _mediator;
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<TrackKomsCommandHandler> _logger;
    private readonly ISegmentService _segmentService;
    private readonly IAthleteService _athleteService;
    private readonly IStravaAthleteService _stravaAthleteService;

    public TrackKomsCommandHandler(IMediator mediator, IKOMUnitOfWork komUoW, ILogger<TrackKomsCommandHandler> logger, ISegmentService segmentService, IAthleteService athleteService, IStravaAthleteService stravaAthleteService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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

            await TrackKomsForAthleteAsync(athlete);
        }

        return Result.Ok();
    }

    protected async Task TrackKomsForAthleteAsync(AthleteEntity athlete, int retry = 0)
    {
        var logPrefix = $"{nameof(TrackKomsForAthleteAsync)} ";

        if (retry >= MAX_RETRY_COUNT)
        {
            _logger.LogWarning(logPrefix + "max retry count: {maxRetryCount} exceeded", MAX_RETRY_COUNT);
            return;
        }

        var athleteId = athlete.AthleteId;
        var token = await GetTokenAsync(athleteId);
        if (token == null) return;

        var acutalKomsRes = await _stravaAthleteService.GetAthleteKomsAsync(athleteId, token);

        if (!acutalKomsRes.IsSuccess)
        {
            // Try again when Unauthorized
            if (acutalKomsRes.HasError<Interfaces.Services.Strava.GetAthleteKomsError>(x => x.Message == Interfaces.Services.Strava.GetAthleteKomsError.Unauthorized))
            {
                await TrackKomsForAthleteAsync(athlete, ++retry);
            }

            // Logging done in Strava.API.Client
            return;
        }

        var actualKoms = acutalKomsRes.Value;

        var lastKomsSummaryEfforts = await _segmentService.GetLastKomsSummaryEffortsAsync(athleteId);
        var firstTrack = lastKomsSummaryEfforts == null;
        var lastKomsEfforts = lastKomsSummaryEfforts?.Where(x => x.SummarySegmentEffort.Kom).Select(x => x.SegmentEffort).ToArray()
            ?? Enumerable.Empty<SegmentEffortEntity>();

        var comparedEfforts = _segmentService.CompareEfforts(actualKoms.Select(x => x.Item1), lastKomsEfforts, firstTrack);

        if (comparedEfforts.AnyChanges || firstTrack)
        {
            await _segmentService.CheckNewKomsAreReturnedAsync(comparedEfforts);

            var lastSegmetns = lastKomsSummaryEfforts?.Select(x => x.Segment!).ToArray()
                ?? Enumerable.Empty<SegmentEntity>();

            var newSegments = actualKoms.Select(x => x.Item2).Except(lastSegmetns, new SegmentEntityComparer());
            var newEfforts = actualKoms.Select(x => x.Item1).Except(lastKomsEfforts, new SegmentEffortEntityComparer());           

            // TODO: transaction
            await _segmentService.AddSegmentsIfNotExistsAsync(newSegments);
            await _segmentService.AddSegmentEffortsIfNotExistsAsync(newEfforts);
            await _segmentService.AddNewKomsSummaryWithEffortsAsync(athleteId, comparedEfforts);

            await _komUoW.SaveChangesAsync();

            await _mediator.Publish(new TrackKomsCompletedNotification
            {
                Athlete = athlete,
                ComparedEfforts = comparedEfforts
            });
        }
    }

    protected async Task<string?> GetTokenAsync(int athleteId)
    {
        var getValidTokenRes = await _athleteService.GetValidTokenAsync(athleteId);

        return getValidTokenRes.IsSuccess ?
            getValidTokenRes.Value?.AccessToken
            : null;
    }
}

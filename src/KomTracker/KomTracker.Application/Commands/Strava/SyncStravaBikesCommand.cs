using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Services;
using MediatR;
using IStravaGearService = KomTracker.Application.Interfaces.Services.Strava.IGearService;

namespace KomTracker.Application.Commands.Strava;

/// <summary>
/// Sync one athlete's Strava gear (bikes, incl. retired) into strava.bike (1:1 mirror). Gear only —
/// does NOT touch any auto-sync flag (bike auto-sync is toggled on Account) nor activity sync.
/// Surfaces rate-limit / auth failures to the caller.
/// </summary>
public class SyncStravaBikesCommand : IRequest<Result>
{
    public int AthleteId { get; set; }
}

public class SyncStravaBikesCommandHandler : IRequestHandler<SyncStravaBikesCommand, Result>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaGearService _gearService;

    public SyncStravaBikesCommandHandler(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaGearService gearService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _gearService = gearService ?? throw new ArgumentNullException(nameof(gearService));
    }

    public async Task<Result> Handle(SyncStravaBikesCommand request, CancellationToken cancellationToken)
    {
        var tokenRes = await _athleteService.GetValidTokenAsync(request.AthleteId);
        if (tokenRes.IsFailed)
        {
            return Result.Fail($"No valid Strava token for athlete {request.AthleteId}.");
        }

        // Also hydrate bikes seen in past activities (retired bikes aren't in GET /athlete bikes[]).
        var activityRepo = _komUoW.GetRepository<IActivityRepository>();
        var extraGearIds = (await activityRepo.GetDistinctBikeGearIdsAsync(request.AthleteId)).ToList();

        var bikesRes = await _gearService.GetAthleteBikesAsync(request.AthleteId, tokenRes.Value.AccessToken, extraGearIds);
        if (bikesRes.IsFailed)
        {
            return Result.Fail(bikesRes.Errors);
        }

        var stravaBikeRepo = _komUoW.GetRepository<IStravaBikeRepository>();

        await stravaBikeRepo.UpsertAthleteBikesAsync(request.AthleteId, bikesRes.Value.ToList());

        return Result.Ok();
    }
}

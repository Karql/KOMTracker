using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using IStravaGearService = KomTracker.Application.Interfaces.Services.Strava.IGearService;

namespace KomTracker.Application.Services;

public class StravaBikeSyncService : IStravaBikeSyncService
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaGearService _gearService;

    public StravaBikeSyncService(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaGearService gearService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _gearService = gearService ?? throw new ArgumentNullException(nameof(gearService));
    }

    public async Task<Result> SyncAthleteBikesAsync(int athleteId, CancellationToken cancellationToken)
    {
        var tokenRes = await _athleteService.GetValidTokenAsync(athleteId);
        if (tokenRes.IsFailed)
        {
            return Result.Fail($"No valid Strava token for athlete {athleteId}.");
        }

        // Also hydrate bikes seen in past activities (retired bikes aren't in GET /athlete bikes[]).
        var activityRepo = _komUoW.GetRepository<IActivityRepository>();
        var extraGearIds = (await activityRepo.GetDistinctBikeGearIdsAsync(athleteId)).ToList();

        var bikesRes = await _gearService.GetAthleteBikesAsync(athleteId, tokenRes.Value.AccessToken, extraGearIds);
        if (bikesRes.IsFailed)
        {
            return Result.Fail(bikesRes.Errors);
        }

        var stravaBikeRepo = _komUoW.GetRepository<IStravaBikeRepository>();

        await stravaBikeRepo.UpsertAthleteBikesAsync(athleteId, bikesRes.Value.ToList());

        return Result.Ok();
    }
}

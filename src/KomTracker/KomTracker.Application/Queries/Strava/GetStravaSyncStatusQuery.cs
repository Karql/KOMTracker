using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Application.Models.Strava;
using KomTracker.Application.Services;
using MediatR;

namespace KomTracker.Application.Queries.Strava;

/// <summary>Per-athlete Strava sync state (drives the page status line + empty-state).</summary>
public class GetStravaSyncStatusQuery : IRequest<StravaSyncStatusModel>
{
    public int AthleteId { get; set; }
}

public class GetStravaSyncStatusQueryHandler : IRequestHandler<GetStravaSyncStatusQuery, StravaSyncStatusModel>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;

    public GetStravaSyncStatusQueryHandler(IKOMUnitOfWork komUoW, IAthleteService athleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
    }

    public async Task<StravaSyncStatusModel> Handle(GetStravaSyncStatusQuery request, CancellationToken cancellationToken)
    {
        var athleteSyncRepo = _komUoW.GetRepository<IAthleteSyncRepository>();
        var stravaBikeRepo = _komUoW.GetRepository<IStravaBikeRepository>();

        var athleteSync = await athleteSyncRepo.GetAsync(request.AthleteId);
        var stravaBikeCount = (await stravaBikeRepo.GetByAthleteAsync(request.AthleteId)).Count();

        var tokenRes = await _athleteService.GetValidTokenAsync(request.AthleteId);
        var scopes = tokenRes.IsSuccess
            ? (tokenRes.Value.Scope?.Split(',') ?? Array.Empty<string>())
            : Array.Empty<string>();

        return new StravaSyncStatusModel
        {
            BikesEnabled = athleteSync?.BikesEnabled ?? false,
            ActivitiesEnabled = athleteSync?.ActivitiesEnabled ?? false,
            HasActivityReadAll = scopes.Contains(Constants.Strava.ScopeActivityReadAll),
            StravaBikeCount = stravaBikeCount,
            Scopes = scopes
        };
    }
}

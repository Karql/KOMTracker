using FluentResults;
using KomTracker.Application.Commands.Tracking;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Models.Configuration;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Segment;
using MediatR;
using Microsoft.Extensions.Logging;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;
namespace KomTracker.Application.Commands.Club;

public class RefreshClubsCommand : IRequest<Result>
{

}

public class RefreshClubsCommandHandler : IRequestHandler<RefreshClubsCommand, Result>
{
    private const int MAX_RETRY_COUNT = 2;

    private readonly IKOMUnitOfWork _komUoW;
    private readonly ILogger<RefreshClubsCommandHandler> _logger;
    private readonly ApplicationConfiguration _applicationConfiguration;
    private readonly IClubService _clubService;
    private readonly IAthleteService _athleteService;
    private readonly IStravaAthleteService _stravaAthleteService;

    public RefreshClubsCommandHandler(IKOMUnitOfWork komUoW, ILogger<RefreshClubsCommandHandler> logger, ApplicationConfiguration applicationConfiguration, IAthleteService athleteService, IClubService clubService, IStravaAthleteService stravaAthleteService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _applicationConfiguration = applicationConfiguration ?? throw new ArgumentNullException(nameof(applicationConfiguration));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _clubService = clubService ?? throw new ArgumentNullException(nameof(clubService));
        _stravaAthleteService = stravaAthleteService ?? throw new ArgumentNullException(nameof(stravaAthleteService));
    }

    public async Task<Result> Handle(RefreshClubsCommand request, CancellationToken cancellationToken)
    {
        var athlets = await _athleteService.GetAllAthletesAsync();

        foreach (var athlete in athlets)
        {
            if (cancellationToken.IsCancellationRequested) return Result.Ok(); // TODO: OK?
           
            if (!await RefreshClubsForAthlete(athlete))
                return Result.Fail($"{nameof(RefreshClubsCommand)} execution interrupted!");
        }

        return Result.Ok();
    }

    /// <returns>If loop should be continued</returns>
    protected async Task<bool> RefreshClubsForAthlete(AthleteEntity athlete, int retry = 0)
    {
        var logPrefix = $"{nameof(RefreshClubsForAthlete)} ";

        if (retry >= MAX_RETRY_COUNT)
        {
            _logger.LogWarning(logPrefix + "max retry count: {maxRetryCount} exceeded", MAX_RETRY_COUNT);
            return true;
        }

        var athleteId = athlete.AthleteId;
        var token = await GetTokenAsync(athleteId);
        if (token == null) return true;

        var clubsRes = await _stravaAthleteService.GetAthleteClubsAsync(athleteId, token);

        if (!clubsRes.IsSuccess)
        {
            var errorMessage = clubsRes.Errors.OfType<Interfaces.Services.Strava.GetAthleteClubsError>().FirstOrDefault()?.Message;

            // Try again when Unauthorized
            if (errorMessage == Interfaces.Services.Strava.GetAthleteClubsError.Unauthorized)
            {
                await RefreshClubsForAthlete(athlete, ++retry);
            }

            // Break execution on TooManyRequest
            if (errorMessage == Interfaces.Services.Strava.GetAthleteClubsError.TooManyRequests)
            {
                return false;
            }

            // Logging done in Strava.API.Client
            return true;
        }

        var clubs = clubsRes.Value;

        await _clubService.AddOrUpdateClubsAsync(clubs);
        await _clubService.SyncAthleteClubsAsync(athleteId, clubs);
        await _komUoW.SaveChangesAsync();

        return true;
    }

    protected async Task<string?> GetTokenAsync(int athleteId)
    {
        var getValidTokenRes = await _athleteService.GetValidTokenAsync(athleteId);

        return getValidTokenRes.IsSuccess ?
            getValidTokenRes.Value?.AccessToken
            : null;
    }
}
using FluentResults;
using KomTracker.Application.Services;
using KomTracker.Domain.Entities.Athlete;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IStravaAthleteService = KomTracker.Application.Interfaces.Services.Strava.IAthleteService;

namespace KomTracker.Application.Commands.Account;

/// <summary>
/// Refreshes stored athlete profiles (name, avatar, etc.) from Strava for all athletes.
/// Athlete data is otherwise only updated at login, so long-inactive users keep stale data
/// (most visibly broken avatar links).
/// </summary>
public class RefreshAthletesCommand : IRequest<Result>
{
}

public class RefreshAthletesCommandHandler : IRequestHandler<RefreshAthletesCommand, Result>
{
    private readonly ILogger<RefreshAthletesCommandHandler> _logger;
    private readonly IAthleteService _athleteService;
    private readonly IStravaAthleteService _stravaAthleteService;

    public RefreshAthletesCommandHandler(ILogger<RefreshAthletesCommandHandler> logger, IAthleteService athleteService, IStravaAthleteService stravaAthleteService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaAthleteService = stravaAthleteService ?? throw new ArgumentNullException(nameof(stravaAthleteService));
    }

    public async Task<Result> Handle(RefreshAthletesCommand request, CancellationToken cancellationToken)
    {
        var logPrefix = $"{nameof(RefreshAthletesCommandHandler)} ";
        var athletes = await _athleteService.GetAllAthletesAsync();

        foreach (var athlete in athletes)
        {
            if (cancellationToken.IsCancellationRequested)
                return Result.Ok();

            try
            {
                var token = await GetTokenAsync(athlete.AthleteId);
                if (token == null) continue;

                var getAthleteRes = await _stravaAthleteService.GetAthleteAsync(athlete.AthleteId, token);

                if (!getAthleteRes.IsSuccess)
                {
                    var errorMessage = getAthleteRes.Errors.OfType<Interfaces.Services.Strava.GetAthleteError>().FirstOrDefault()?.Message;

                    // Break execution on TooManyRequest
                    if (errorMessage == Interfaces.Services.Strava.GetAthleteError.TooManyRequests)
                        return Result.Fail($"{nameof(RefreshAthletesCommand)} execution interrupted!");

                    // Logging done in Strava.API.Client
                    continue;
                }

                await _athleteService.AddOrUpdateAthleteAsync(getAthleteRes.Value);
            }
            catch (Exception ex)
            {
                // Isolate failures: one bad athlete must not abort the whole run.
                _logger.LogError(ex, logPrefix + "failed for athlete {athleteId} - skipping", athlete.AthleteId);
            }
        }

        return Result.Ok();
    }

    protected async Task<string?> GetTokenAsync(int athleteId)
    {
        var getValidTokenRes = await _athleteService.GetValidTokenAsync(athleteId);

        return getValidTokenRes.IsSuccess ?
            getValidTokenRes.Value?.AccessToken
            : null;
    }
}

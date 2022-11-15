using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Persistence.Repositories;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Club;
using KomTracker.Domain.Entities.Segment;
using KomTracker.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;

namespace KomTracker.Application.Services;

public class AthleteService : IAthleteService
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IStravaTokenService _stravaTokenService;

    public AthleteService(IKOMUnitOfWork komUoW, IStravaTokenService stravaTokenService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _stravaTokenService = stravaTokenService ?? throw new ArgumentNullException(nameof(stravaTokenService));
    }

    public Task<bool> IsAthleteExistsAsync(int athleteId)
    {
        return _komUoW
            .GetRepository<IAthleteRepository>()
            .IsAthleteExistsAsync(athleteId);
    }

    public Task<AthleteEntity> GetAthleteAsync(int athleteId)
    {
        return _komUoW
            .GetRepository<IAthleteRepository>()
            .GetAthleteAsync(athleteId);
    }

    public Task AddOrUpdateAthleteAsync(AthleteEntity athlete)
    {
        return _komUoW
            .GetRepository<IAthleteRepository>()
            .AddOrUpdateAthleteAsync(athlete);
    }

    public Task AddOrUpdateTokenAsync(TokenEntity token)
    {
        return _komUoW
            .GetRepository<IAthleteRepository>()
            .AddOrUpdateTokenAsync(token);
    }

    public async Task<IEnumerable<AthleteEntity>> GetAllAthletesAsync()
    {
        return await _komUoW
            .GetRepository<IAthleteRepository>()
            .GetAllAthletesAsync();
    }

    public async Task<Result<TokenEntity>> GetValidTokenAsync(int athleteId)
    {
        var token = await GetTokenAsync(athleteId);

        if (token == null)
        {
            await DeactivateAthleteAsync(athleteId);
            return Result.Fail<TokenEntity>(new GetValidTokenError(GetValidTokenError.NoTokenInDB));
        }

        if (IsValidToken(token))
        {
            return Result.Ok(token);
        }

        var refreshTokenResult = await _stravaTokenService.RefreshAsync(token);

        if (refreshTokenResult.IsSuccess)
        {
            var newToken = refreshTokenResult.Value;
            await AddOrUpdateTokenAsync(newToken);
            return Result.Ok(newToken);
        }

        await DeactivateAthleteAsync(athleteId);
        return Result.Fail<TokenEntity>(new GetValidTokenError(GetValidTokenError.RefreshFailed));
    }

    protected Task<TokenEntity> GetTokenAsync(int athleteId)
    {
        return _komUoW
            .GetRepository<IAthleteRepository>()
            .GetTokenAsync(athleteId);
    }

    protected bool IsValidToken(TokenEntity token)
    {
        return token.ExpiresAt > DateTime.UtcNow;
    }

    protected Task DeactivateAthleteAsync(int athleteId)
    {
        return _komUoW.GetRepository<IAthleteRepository>()
            .DeactivateAthleteAsync(athleteId);
    }
}

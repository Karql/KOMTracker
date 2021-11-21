using FluentResults;
using KomTracker.Application.Errors.Account;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;

namespace KomTracker.Application.Services;

public class AccountService : IAccountService
{
    private static readonly HashSet<string> REQUIRED_SCOPES = new()
    {
        "read",
        "activity:read",
        "profile:read_all"
    };

    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaTokenService _stravaTokenService;
    private readonly IIdentityUserService _userService;

    public AccountService(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaTokenService stravaTokenService, IIdentityUserService userService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaTokenService = stravaTokenService ?? throw new ArgumentNullException(nameof(stravaTokenService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Connect(string code, string scope)
    {
        // TODO: transaction
        if (!VerifyRequiredScope(scope))
        {
            return Result.Fail(new ConnectError(ConnectError.NoRequiredScope));
        }

        var exchangeResult = await _stravaTokenService.ExchangeAsync(code, scope);

        if (!exchangeResult.IsSuccess)
        {
            return Result.Fail(new ConnectError(ConnectError.InvalidCode));
        }

        var (athlete, token) = exchangeResult.Value;

        await _athleteService.AddOrUpdateAthleteAsync(athlete);
        await _athleteService.AddOrUpdateTokenAsync(token);

        if (!await _userService.IsUserExistsAsync(athlete.AthleteId))
        {
            await _userService.AddUserAsync(athlete);
        }

        await _komUoW.SaveChangesAsync();

        // TODO: Login

        return Result.Ok();
    }

    protected bool VerifyRequiredScope(string scope)
    {
        var scopes = scope?.Split(",") ?? Enumerable.Empty<string>();

        return REQUIRED_SCOPES.All(x => scopes.Contains(x));
    }
}

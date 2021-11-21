using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using KomTracker.Application.Errors.Account;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;
using IIdentityUserService = KomTracker.Application.Interfaces.Services.Identity.IUserService;

namespace KomTracker.Application.Commands.Account;

public class ConnectCommand : IRequest<Result>
{
    public string Code { get; set; }
    public string Scope { get; set; }

    public ConnectCommand(string code, string scope)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }
}

public class ConnectCommandHandler : IRequestHandler<ConnectCommand, Result>
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

    public ConnectCommandHandler(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaTokenService stravaTokenService, IIdentityUserService userService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaTokenService = stravaTokenService ?? throw new ArgumentNullException(nameof(stravaTokenService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<Result> Handle(ConnectCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code;
        var scope = request.Scope;

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

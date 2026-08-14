using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Services;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IStravaTokenService = KomTracker.Application.Interfaces.Services.Strava.ITokenService;

namespace KomTracker.Application.Commands.Account;

/// <summary>
/// Completes a Strava scope-escalation: exchanges the OAuth code and overwrites the athlete's stored
/// token (incl. the widened <c>Scope</c>). Isolated from login — no IdentityServer session, no user
/// creation. The athlete is taken from Strava's exchange response, so it only ever affects that athlete.
/// </summary>
public class UpgradeScopeCommand : IRequest<Result<UpgradeScopeResult>>
{
    public string Code { get; set; }
    public string Scope { get; set; }

    public UpgradeScopeCommand(string code, string scope)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }
}

public class UpgradeScopeResult
{
    /// <summary>Whether the granted scope includes activity:read_all (drives the UI "granted" vs "denied").</summary>
    public bool HasActivityReadAll { get; set; }
}

public class UpgradeScopeCommandHandler : IRequestHandler<UpgradeScopeCommand, Result<UpgradeScopeResult>>
{
    private readonly IKOMUnitOfWork _komUoW;
    private readonly IAthleteService _athleteService;
    private readonly IStravaTokenService _stravaTokenService;

    public UpgradeScopeCommandHandler(IKOMUnitOfWork komUoW, IAthleteService athleteService, IStravaTokenService stravaTokenService)
    {
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _stravaTokenService = stravaTokenService ?? throw new ArgumentNullException(nameof(stravaTokenService));
    }

    public async Task<Result<UpgradeScopeResult>> Handle(UpgradeScopeCommand request, CancellationToken cancellationToken)
    {
        var exchangeResult = await _stravaTokenService.ExchangeAsync(request.Code, request.Scope);
        if (!exchangeResult.IsSuccess)
        {
            return Result.Fail("Strava code exchange failed.");
        }

        var (_, token) = exchangeResult.Value;

        // Always store the freshly-exchanged token — it reflects Strava's current authorization
        // (avoids keeping a stale/invalidated token even if the user declined the wider scope).
        await _athleteService.AddOrUpdateTokenAsync(token);
        await _komUoW.SaveChangesAsync();

        var scopes = request.Scope.Split(',');
        return Result.Ok(new UpgradeScopeResult
        {
            HasActivityReadAll = scopes.Contains(Constants.Strava.ScopeActivityReadAll)
        });
    }
}

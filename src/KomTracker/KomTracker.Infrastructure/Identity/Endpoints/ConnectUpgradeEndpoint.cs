using IdentityServer4.Hosting;
using KomTracker.Application.Commands.Account;
using KomTracker.Infrastructure.Identity.Endpoints.Results;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints;

/// <summary>
/// Strava callback for the scope-escalation flow. Exchanges the code and overwrites the athlete's
/// token (via <see cref="UpgradeScopeCommand"/>), then redirects back to the app's returnUrl (from
/// state) with a <c>strava_upgrade=granted|denied|error</c> status flag. Does NOT sign the user in.
/// </summary>
internal class ConnectUpgradeEndpoint : IEndpointHandler
{
    internal const string Code_ParamName = "code";
    internal const string Scope_ParamName = "scope";
    internal const string State_ParamName = "state";
    internal const string StatusQueryName = "strava_upgrade";

    private readonly IMediator _mediator;

    public ConnectUpgradeEndpoint(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        string code = context.Request.Query[Code_ParamName];
        string scope = context.Request.Query[Scope_ParamName];
        string state = context.Request.Query[State_ParamName];

        if (string.IsNullOrEmpty(code)) return new BadRequestResult($"No {Code_ParamName} parameter!");
        if (string.IsNullOrEmpty(scope)) return new BadRequestResult($"No {Scope_ParamName} parameter!");
        if (string.IsNullOrEmpty(state)) return new BadRequestResult($"No {State_ParamName} parameter!");

        var returnUrl = Encoding.UTF8.GetString(Convert.FromBase64String(state));

        var res = await _mediator.Send(new UpgradeScopeCommand(code, scope));

        // The token now reflects Strava's current authorization (up or down). The web page reads the
        // refreshed access level to craft the message; here we only report success vs failure.
        var status = res.IsSuccess ? "ok" : "error";

        return new RedirectResult(AppendStatus(returnUrl, status));
    }

    private static string AppendStatus(string returnUrl, string status)
    {
        var separator = returnUrl.Contains('?') ? "&" : "?";
        return $"{returnUrl}{separator}{StatusQueryName}={status}";
    }
}

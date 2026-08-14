using IdentityServer4.Hosting;
using KomTracker.Infrastructure.Identity.Configurations;
using KomTracker.Infrastructure.Identity.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using Strava.API.Client.Configurations;
using System;
using System.Linq;
using System.Threading.Tasks;
using static KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.Infrastructure.Identity.Endpoints;

/// <summary>
/// Opt-in Strava scope escalation: redirect to Strava's authorize with the wider sync scopes
/// (incl. activity:read_all) + approval_prompt=force. Standalone side-flow — the callback
/// (<see cref="ConnectUpgradeEndpoint"/>) only re-stores the token, no IdentityServer session.
/// </summary>
internal class UpgradeEndpoint : IEndpointHandler
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly StravaApiClientConfiguration _stravaApiClientConfiguration;

    public UpgradeEndpoint(IdentityConfiguration identityConfiguration, StravaApiClientConfiguration stravaApiClientConfiguration)
    {
        _identityConfiguration = identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration));
        _stravaApiClientConfiguration = stravaApiClientConfiguration ?? throw new ArgumentNullException(nameof(stravaApiClientConfiguration));
    }

    internal const string BasicMode = "basic";

    public Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        string returnUrl = context.Request.Query["returnUrl"];
        string mode = context.Request.Query["mode"];

        if (string.IsNullOrEmpty(returnUrl))
        {
            return Task.FromResult<IEndpointResult>(new BadRequestResult("No returnUrl parameter!"));
        }

        // Open-redirect guard: only bounce back to a known web origin.
        if (!IsAllowedReturnUrl(returnUrl))
        {
            return Task.FromResult<IEndpointResult>(new BadRequestResult("Invalid returnUrl!"));
        }

        // mode=basic → re-auth WITHOUT activity:read_all (revoke private rides); otherwise full (grant).
        var scopes = mode == BasicMode
            ? KomTracker.Application.Constants.Strava.BasicScopes
            : KomTracker.Application.Constants.Strava.AuthorizeScopes;

        var connectRedirectUri = $"{_identityConfiguration.IdentityUrl}{ProtocolRoutePaths.ConnectUpgrade}";

        var url = StravaAuthorizeUrl.Build(
            _stravaApiClientConfiguration.ClientID,
            scopes,
            approvalPrompt: "force",
            redirectUri: connectRedirectUri,
            returnUrl: returnUrl);

        return Task.FromResult<IEndpointResult>(new RedirectResult(url));
    }

    private bool IsAllowedReturnUrl(string returnUrl)
    {
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out _))
        {
            return false;
        }

        // Same trust model as StartWithRedirectUriValidator: allow only URLs under a configured
        // redirect-uri prefix (RedirectUris are prefixes like "https://localhost", not exact URIs).
        return (_identityConfiguration.RedirectUris ?? Enumerable.Empty<string>())
            .Any(prefix => returnUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}

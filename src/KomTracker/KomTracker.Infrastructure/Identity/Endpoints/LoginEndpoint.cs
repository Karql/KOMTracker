﻿using IdentityServer4.Hosting;
using KomTracker.Infrastructure.Identity.Configurations;
using KomTracker.Infrastructure.Identity.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using Strava.API.Client.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KomTracker.Infrastructure.Shared.Identity.Constants;

namespace KomTracker.Infrastructure.Identity.Endpoints;

internal class LoginEndpoint : IEndpointHandler
{
    private readonly IdentityConfiguration _identityConfiguration;
    private readonly StravaApiClientConfiguration _stravaApiClientConfiguration;

    public LoginEndpoint(IdentityConfiguration identityConfiguration,StravaApiClientConfiguration stravaApiClientConfiguration)
    {
        _identityConfiguration = identityConfiguration ?? throw new ArgumentNullException(nameof(identityConfiguration));
        _stravaApiClientConfiguration = stravaApiClientConfiguration ?? throw new ArgumentNullException(nameof(stravaApiClientConfiguration));
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        string returnUrl = context.Request.Query["returnUrl"];

        if (string.IsNullOrEmpty(returnUrl)) return new BadRequestResult($"No {nameof(returnUrl)} parameter!");

        return new RedirectResult(GetStravaAuthUrl(context, returnUrl));
    }

    private string GetStravaAuthUrl(HttpContext context, string returnUrl)
    {
        var request = context.Request;

        var clientId = _stravaApiClientConfiguration.ClientID;
        var scope = string.Join(",", KomTracker.Application.Constants.Strava.RequiredScopes);
        var connectRedirectUri = $"{_identityConfiguration.IdentityUrl}{ProtocolRoutePaths.Connect}";
        var appReturnUrlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(returnUrl));

        return $"https://www.strava.com/oauth/authorize?approval_prompt=auto&scope={scope}&client_id={clientId}&response_type=code&redirect_uri={connectRedirectUri}&state={appReturnUrlBase64}";
    }
}

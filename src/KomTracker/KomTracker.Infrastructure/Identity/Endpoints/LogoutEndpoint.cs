using IdentityServer4.Hosting;
using KomTracker.Infrastructure.Identity.Endpoints.Results;
using KomTracker.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints;
internal class LogoutEndpoint : IEndpointHandler
{
    private readonly IIdentityService _identityService;

    public LogoutEndpoint(IIdentityService identityService)
    {
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        string logoutId = context.Request.Query["logoutId"];

        if (string.IsNullOrEmpty(logoutId)) return new BadRequestResult($"No {nameof(logoutId)} parameter!");

        var user = context.User;

        var returnUrl = await _identityService.LogoutAsync(logoutId, user);

        if (string.IsNullOrEmpty(returnUrl))
        {
            return new BadRequestResult($"Invalid {nameof(logoutId)} parameter!");
        }

        return new RedirectResult(returnUrl);
    }
}

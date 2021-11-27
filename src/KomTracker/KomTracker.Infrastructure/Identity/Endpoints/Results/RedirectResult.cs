using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints.Results;

internal class RedirectResult : IEndpointResult
{
    private readonly string _redirectUrl;

    public RedirectResult(string returnUrl)
    {
        _redirectUrl = returnUrl ?? throw new ArgumentNullException(nameof(returnUrl));
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.Redirect(_redirectUrl);
    }
}

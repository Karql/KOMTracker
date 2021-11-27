using IdentityServer4.Hosting;
using KomTracker.Application.Commands.Account;
using KomTracker.Application.Errors.Account;
using KomTracker.Infrastructure.Identity.Endpoints.Results;
using KomTracker.Infrastructure.Identity.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints;

internal class ConnectEndpoint : IEndpointHandler
{
    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;

    public ConnectEndpoint(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        var code = context.Request.Query["code"];
        var scope = context.Request.Query["scope"];
        var state = context.Request.Query["state"];

        if (string.IsNullOrEmpty(code)) return new BadRequestResult($"No {nameof(code)} parameter!");
        if (string.IsNullOrEmpty(scope)) return new BadRequestResult($"No {nameof(scope)} parameter!");
        if (string.IsNullOrEmpty(state)) return new BadRequestResult($"No {nameof(state)} parameter!");

        var res = await _mediator.Send(new ConnectCommand(code, scope));

        if (!res.IsSuccess)
        {
            if (!res.HasError<ConnectError>())
            {
                throw new Exception($"{nameof(ConnectEndpoint)} {nameof(ConnectCommand)} failed!");
            }

            // TODO: Better handling (redirect with error message)
            var error = (ConnectError)res.Errors.FirstOrDefault();
            return new BadRequestResult(error.Message);
        }

        var returnUrl = await _identityService.LoginAsync(res.Value.AthleteId, state);

        return new RedirectResult(returnUrl);
    }
}

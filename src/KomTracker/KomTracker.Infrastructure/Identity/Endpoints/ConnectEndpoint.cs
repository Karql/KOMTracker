using IdentityServer4.Hosting;
using KomTracker.Application.Commands.Account;
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
    internal const string Code_ParamName = "code";
    internal const string Scope_ParamName = "scope";
    internal const string State_ParamName = "state";

    private readonly IMediator _mediator;
    private readonly IIdentityService _identityService;

    public ConnectEndpoint(IMediator mediator, IIdentityService identityService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        string code = context.Request.Query[Code_ParamName];
        string scope = context.Request.Query[Scope_ParamName];
        string state = context.Request.Query[State_ParamName];

        if (string.IsNullOrEmpty(code)) return new BadRequestResult($"No {Code_ParamName} parameter!");
        if (string.IsNullOrEmpty(scope)) return new BadRequestResult($"No {Scope_ParamName} parameter!");
        if (string.IsNullOrEmpty(state)) return new BadRequestResult($"No {State_ParamName} parameter!");

        var res = await _mediator.Send(new ConnectCommand(code, scope));

        if (!res.IsSuccess)
        {
            if (!res.HasError<ConnectCommandError>())
            {
                throw new Exception($"{nameof(ConnectEndpoint)} {nameof(ConnectCommand)} failed!");
            }

            // TODO: Better handling (redirect with error message)
            var error = (ConnectCommandError)res.Errors.FirstOrDefault();
            return new BadRequestResult(error.Message);
        }

        var returnUrl = await _identityService.LoginAsync(res.Value.AthleteId, state);

        return new RedirectResult(returnUrl);
    }
}

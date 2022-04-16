using IdentityServer4.Hosting;
using KomTracker.Application.Interfaces.Services.Identity;
using KomTracker.Infrastructure.Identity.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KomTracker.Infrastructure.Identity.Endpoints;
internal class ConfirmEmailChangeEndpoint : IEndpointHandler
{
    internal const string AthleteId_ParamName = "athlete_id";
    internal const string NewEmail_ParamName = "new_email";
    internal const string Token_ParamName = "token";

    private readonly IUserService _userService;

    public ConfirmEmailChangeEndpoint(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        if ( context.Request.Method == HttpMethods.Get )
        {
            string atletheIdString = context.Request.Query[AthleteId_ParamName];
            string newEmail = context.Request.Query[NewEmail_ParamName];
            string tokenEncoded = context.Request.Query[Token_ParamName];

            if (string.IsNullOrEmpty(atletheIdString)) return new BadRequestResult($"No {AthleteId_ParamName} parameter!");
            if (string.IsNullOrEmpty(newEmail)) return new BadRequestResult($"No {NewEmail_ParamName} parameter!");
            if (string.IsNullOrEmpty(tokenEncoded)) return new BadRequestResult($"No {Token_ParamName} parameter!");

            if (!int.TryParse(atletheIdString, out int athleteId)) return new BadRequestResult($"Wrong {AthleteId_ParamName} parameter!");

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(tokenEncoded));

            var res = await _userService.ConfirmEmailChangeAsync(athleteId, newEmail, token);

            if (!res.IsSuccess)
            {
                if (!res.HasError<ConfirmEmailChangeError>())
                {
                    throw new Exception($"{nameof(ConfirmEmailChangeEndpoint)} {nameof(_userService.ConfirmEmailChangeAsync)} failed!");
                }

                // TODO: Better handling (redirect with error message)
                return new BadRequestResult("Wrong parameters"); // prevent email enumeration
            }

            return new RedirectResult("https://komtracker.karkula.pl"); // todo
        }

        return new MethodNotAllowedResult();
    }
}

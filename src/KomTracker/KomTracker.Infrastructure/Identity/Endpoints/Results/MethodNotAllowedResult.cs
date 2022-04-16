using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints.Results;

internal class MethodNotAllowedResult : BaseErrorResult
{
    public MethodNotAllowedResult()
        : base(StatusCodes.Status405MethodNotAllowed)
    {
        Error = "405 Method Not Allowed";
    }
}

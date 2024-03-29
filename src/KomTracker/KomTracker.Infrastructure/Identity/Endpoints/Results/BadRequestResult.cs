﻿using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints.Results;

internal class BadRequestResult : BaseErrorResult
{
    public BadRequestResult(string error = null, string errorDescription = null)
        : base(StatusCodes.Status400BadRequest)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }
}

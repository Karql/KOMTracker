using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Infrastructure.Identity.Endpoints.Results;
internal abstract class BaseErrorResult : IEndpointResult
{
    public string Error { get; set; }
    public string ErrorDescription { get; set; }

    public BaseErrorResult(int statusCode, string error = null, string errorDescription = null)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }

    public async Task ExecuteAsync(HttpContext context)
    {
        context.Response.StatusCode = 400;
        context.Response.SetNoCache();

        if (!string.IsNullOrEmpty(Error))
        {
            var dto = new ResultDto
            {
                error = Error,
                error_description = ErrorDescription
            };

            await context.Response.WriteJsonAsync(dto);
        }
    }

    internal class ResultDto
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
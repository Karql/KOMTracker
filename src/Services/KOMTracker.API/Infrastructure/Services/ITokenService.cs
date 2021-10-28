using FluentResults;
using KOMTracker.API.Models.Strava;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public interface ITokenService
    {
        Task<Result<(AthleteModel, TokenModel)>> ExchangeTokenAsync(string code, string scope);
    }
}

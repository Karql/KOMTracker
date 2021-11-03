using FluentResults;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KOMTracker.API.Infrastructure.Services
{
    public interface ITokenService
    {
        /// <summary>
        /// Exchange code for token
        /// </summary>
        Task<Result<(AthleteModel, TokenModel)>> ExchangeAsync(string code, string scope);

        /// <summary>
        /// Refresh token
        /// </summary>
        Task<Result<TokenModel>> RefreshAsync(TokenModel token);
    }
}

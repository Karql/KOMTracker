using FluentResults;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KomTracker.Application.Interfaces.Services.Strava;

public interface ITokenService
{
    /// <summary>
    /// Exchange code for token
    /// </summary>
    Task<Result<(AthleteEntity, TokenEntity)>> ExchangeAsync(string code, string scope);

    /// <summary>
    /// Refresh token
    /// </summary>
    Task<Result<TokenEntity>> RefreshAsync(TokenEntity token);
}

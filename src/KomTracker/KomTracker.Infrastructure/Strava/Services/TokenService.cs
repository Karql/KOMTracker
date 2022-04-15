using AutoMapper;
using FluentResults;
using KomTracker.Application.Interfaces.Persistence;
using KomTracker.Application.Interfaces.Services.Strava;
using KomTracker.Domain.Entities.Athlete;
using KomTracker.Domain.Entities.Token;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KomTracker.Infrastructure.Strava.Services;

public class TokenService : ITokenService
{
    private readonly IMapper _mapper;
    private readonly IKOMUnitOfWork _komUoW;
    private readonly ITokenApi _tokenApi;

    public TokenService(IMapper mapper, IKOMUnitOfWork komUoW, ITokenApi tokenApi)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
        _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
    }

    public async Task<Result<(AthleteEntity, TokenEntity)>> ExchangeAsync(string code, string scope)
    {
        var exchangeResult = await _tokenApi.ExchangeAsync(code);
        if (!exchangeResult.IsSuccess)
        {
            if (exchangeResult.HasError<ApiModel.Token.Error.ExchangeError>(x => x.Message == ApiModel.Token.Error.ExchangeError.InvalidCode))
            {
                return Result.Fail(new ExchangeError(ExchangeError.InvalidCode));
            }

            throw new Exception($"{nameof(_tokenApi.ExchangeAsync)} failed!");
        }

        var apiTokenWithAthlete = exchangeResult.Value;
        var athlete = _mapper.Map<AthleteEntity>(apiTokenWithAthlete.Athlete);
        var token = _mapper.Map<TokenEntity>(apiTokenWithAthlete);
        token.Scope = scope;

        return Result.Ok((athlete, token));
    }

    public async Task<Result<TokenEntity>> RefreshAsync(TokenEntity token)
    {
        var refreshResult = await _tokenApi.RefreshAsync(token.RefreshToken);

        if (!refreshResult.IsSuccess)
        {
            if (refreshResult.HasError<ApiModel.Token.Error.RefreshError>(x =>
                x.Message == ApiModel.Token.Error.RefreshError.InvalidRefreshToken))
            {
                return Result.Fail(new RefreshError(RefreshError.InvalidRefreshToken));
            }

            throw new Exception($"{nameof(_tokenApi.RefreshAsync)} failed!");
        }

        var newToken = _mapper.Map<TokenEntity>(refreshResult.Value);
        newToken.AthleteId = token.AthleteId;
        newToken.Scope = token.Scope;

        return Result.Ok(newToken);
    }
}

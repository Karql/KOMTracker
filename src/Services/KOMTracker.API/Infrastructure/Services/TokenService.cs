using AutoMapper;
using FluentResults;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using KOMTracker.API.Models.Token.Error;
using Strava.API.Client.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly IMapper _mapper;
        private readonly ITokenApi _tokenApi;

        public TokenService(IMapper mapper, ITokenApi tokenApi)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _tokenApi = tokenApi ?? throw new ArgumentNullException(nameof(tokenApi));
        }

        public async Task<Result<(AthleteModel, TokenModel)>> ExchangeAsync(string code, string scope)
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
            var athlete = _mapper.Map<AthleteModel>(apiTokenWithAthlete.Athlete);
            var token = _mapper.Map<TokenModel>(apiTokenWithAthlete);
            token.Scope = scope;

            return Result.Ok((athlete, token));
        }
    }
}

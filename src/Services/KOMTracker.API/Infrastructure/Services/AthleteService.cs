using FluentResults;
using KOMTracker.API.DAL;
using KOMTracker.API.DAL.Repositories;
using KOMTracker.API.Models.Athlete;
using KOMTracker.API.Models.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiModel = Strava.API.Client.Model;

namespace KOMTracker.API.Infrastructure.Services
{
    public class AthleteService : IAthleteService
    {
        private readonly IKOMUnitOfWork _komUoW;
        private readonly ITokenService _tokenService;

        public AthleteService(IKOMUnitOfWork komUoW, ITokenService tokenService)
        {
            _komUoW = komUoW ?? throw new ArgumentNullException(nameof(komUoW));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public Task<bool> IsAthleteExistsAsync(int athleteId)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .IsAthleteExistsAsync(athleteId);
        }

        public Task AddOrUpdateAthleteAsync(AthleteModel athlete)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .AddOrUpdateAthleteAsync(athlete);
        }

        public Task AddOrUpdateTokenAsync(TokenModel token)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .AddOrUpdateTokenAsync(token);
        }

        public async Task<Result<TokenModel>> GetValidToken(int athleteId)
        {
            var token = await GetTokenAsync(athleteId);

            if (token == null)
            {
                // TODO: Decativate athlete 
                throw new Exception($"{nameof(GetValidToken)} no token in DB!");
            }

            if (IsValidToken(token))
            {
                return Result.Ok(token);
            }

            var refreshTokenResult = await _tokenService.RefreshAsync(token);

            if (refreshTokenResult.IsSuccess)
            {
                var newToken = refreshTokenResult.Value;
                await AddOrUpdateTokenAsync(newToken);
                return Result.Ok(newToken);
            }

            // TODO: Deactivate athlete on Invalid refresh token
            throw new Exception($"{nameof(GetValidToken)} falied!");
        }

        protected Task<TokenModel> GetTokenAsync(int athleteId)
        {
            return _komUoW
                .GetRepository<IAthleteRepository>()
                .GetTokenAsync(athleteId);
        }

        protected bool IsValidToken(TokenModel token)
        {
            return token.ExpiresAt > DateTime.UtcNow;
        }
    }
}
